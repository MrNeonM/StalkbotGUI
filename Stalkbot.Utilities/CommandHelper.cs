﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Object;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;

namespace StalkbotGUI.Stalkbot.Utilities
{
    /// <summary>
    /// Class containing helper methods for commands
    /// </summary>
    public class CommandHelper
    {
        /// <summary>
        /// Indexes the files if there was a path provided
        /// </summary>
        /// <returns>An array of indexed files</returns>
        public static string[] IndexFiles()
            => !string.IsNullOrEmpty(Config.Instance.FolderPath) ? SearchFiles(Config.Instance.FolderPath) : new[] { "" };

        /// <summary>
        /// Recursive function that crawls through subdirectories
        /// </summary>
        /// <param name="path">Root directory to index</param>
        /// <returns>File array of the files in the current directory</returns>
        private static string[] SearchFiles(string path)
        {
            var files = Directory.GetFiles(path);
            var dirs = Directory.GetDirectories(path);
            return dirs.Length == 0
                ? files
                : dirs.Aggregate(files, (current, dir) => current.Union(SearchFiles(dir)).ToArray());
        }
        
        /// <summary>
        /// Handles a command error
        /// </summary>
        /// <param name="sender">CommandsNext extension object</param>
        /// <param name="e">Event args</param>
        /// <returns>The built task</returns>
        public static async Task CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            // "ignore" non-existent commands
            if (e.Exception.Message.Contains("command was not found"))
            {
                await e.Context.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("❓"));
                return;
            }

            try
            {
                var ex = (ChecksFailedException)e.Exception;
                // check if the command was just disabled
                if (ex.FailedChecks.OfType<RequireEnabled>().Any())
                {
                    Logger.Log(
                        $"{e.Context.User.Username} used {e.Command.Name} command in #{e.Context.Channel.Name}, but it was disabled.",
                        LogLevel.Warning);
                    await e.Context.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("🔕"));
                    return;
                    await e.Context.Message.RespondAsync("der command is disabled lol");
                    return;
                }

                // check if the command was on cooldown
                if (ex.FailedChecks.OfType<CooldownAttribute>().Any())
                {
                    await e.Context.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("⌛"));
                    return;
                }
            }
            catch { /* ignored */ }

            // log an actual error
            await e.Context.Message.CreateReactionAsync(DiscordEmoji.FromUnicode("❌"));
            Logger.Log($"Exception in command {e.Command.Name}! Message: {e.Exception.Message}", LogLevel.Error);
        }
    }
}