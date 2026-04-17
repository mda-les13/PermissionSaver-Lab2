using PermissionSaver.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PermissionSaver.Services
{
    public static class SerializationService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static async Task SerializeAsync(string sourcePath, string outputPath, Action<string> log)
        {
            log($"Начало сериализации: {sourcePath}");

            if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath))
            {
                throw new FileNotFoundException($"Источник не найден: {sourcePath}");
            }

            SerializedNode root;
            if (File.Exists(sourcePath))
            {
                root = await ProcessFileAsync(sourcePath);
            }
            else
            {
                root = await ProcessDirectoryAsync(sourcePath);
            }

            string json = JsonSerializer.Serialize(root, JsonOptions);
            await File.WriteAllTextAsync(outputPath, json, Encoding.UTF8);
            log($"✓ Успешно сохранено в: {outputPath}");
        }

        public static async Task DeserializeAsync(string inputPath, string outputDir, Action<string> log)
        {
            log($"Чтение файла: {inputPath}");

            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"Файл не найден: {inputPath}");
            }

            string json = await File.ReadAllTextAsync(inputPath, Encoding.UTF8);
            var node = JsonSerializer.Deserialize<SerializedNode>(json);

            if (node == null)
                throw new Exception("Ошибка чтения JSON");

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string finalPath = Path.Combine(outputDir, node.Name);
            await RestoreNodeAsync(node, finalPath, log);
            log("✓ Восстановление завершено");
        }

        private static async Task<SerializedNode> ProcessFileAsync(string path)
        {
            byte[] bytes = await File.ReadAllBytesAsync(path);
            return new SerializedNode
            {
                Name = Path.GetFileName(path),
                IsDirectory = false,
                ContentBase64 = Convert.ToBase64String(bytes),
                Permissions = PermissionService.CapturePermissions(path)
            };
        }

        private static async Task<SerializedNode> ProcessDirectoryAsync(string path)
        {
            var node = new SerializedNode
            {
                Name = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar)),
                IsDirectory = true,
                Children = new List<SerializedNode>(),
                Permissions = PermissionService.CapturePermissions(path)
            };

            foreach (var dir in Directory.GetDirectories(path))
            {
                node.Children.Add(await ProcessDirectoryAsync(dir));
            }

            foreach (var file in Directory.GetFiles(path))
            {
                node.Children.Add(await ProcessFileAsync(file));
            }

            return node;
        }

        private static async Task RestoreNodeAsync(SerializedNode node, string fullPath, Action<string> log)
        {
            if (node.IsDirectory)
            {
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);

                PermissionService.ApplyPermissions(fullPath, node.Permissions);
                log($"  Создана папка: {fullPath}");

                if (node.Children != null)
                {
                    foreach (var child in node.Children)
                    {
                        string childPath = Path.Combine(fullPath, child.Name);
                        await RestoreNodeAsync(child, childPath, log);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(node.ContentBase64))
                {
                    byte[] bytes = Convert.FromBase64String(node.ContentBase64);
                    await File.WriteAllBytesAsync(fullPath, bytes);
                    log($"  Восстановлен файл: {fullPath}");
                }

                PermissionService.ApplyPermissions(fullPath, node.Permissions);
            }
        }
    }
}
