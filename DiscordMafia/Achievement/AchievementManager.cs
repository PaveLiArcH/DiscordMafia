﻿using DiscordMafia.Client;
using System;
using System.Collections.Generic;

namespace DiscordMafia.Achievement
{
    public class AchievementManager
    {
        protected readonly Dictionary<string, Achievement> allowedAchievements = new Dictionary<string, Achievement>();

        protected readonly List<Tuple<UserWrapper, Achievement>> achievements = new List<Tuple<UserWrapper, Achievement>>();

        protected readonly Game game;

        public AchievementManager(Game game)
        {
            this.game = game;

            var achievementsToRegister = new List<Achievement>
            {
                new Achievement { Id = Achievement.Id10k, Name = "\U0001F949", Description = "Набрать 10000 очков" },
                new Achievement { Id = Achievement.Id25k, Name = "\U0001F948", Description = "Набрать 25000 очков" },
                new Achievement { Id = Achievement.Id50k, Name = "\U0001F947", Description = "Набрать 50000 очков" },
                new Achievement { Id = Achievement.Id100k, Name = "\U0001F3C6", Description = "Набрать 100000 очков" },
                new Achievement { Id = Achievement.IdCivilKillCom, Name = "\U0000267F", Description = "Убить комиссара, играя за шерифа или горца" },
            };
            foreach (var achievement in achievementsToRegister)
            {
                allowedAchievements.Add(achievement.Id, achievement);
            }
        }

        public bool push(UserWrapper user, string achievementId)
        {
            if (allowedAchievements.ContainsKey(achievementId))
            {
                achievements.Add(new Tuple<UserWrapper, Achievement>(user, allowedAchievements[achievementId]));
                return true;
            }
            return false;
        }

        public bool addInstantly(UserWrapper user, string achievementId)
        {
            if (allowedAchievements.ContainsKey(achievementId))
            {
                var achievement = allowedAchievements[achievementId];
                DB.Achievement dbAchievement = DB.Achievement.findUserAchievement(user.Id, achievementId);
                if (dbAchievement == null)
                {
                    dbAchievement = new DB.Achievement();
                    dbAchievement.userId = user.Id;
                    dbAchievement.achievementId = achievementId;
                    dbAchievement.achievedAt = DateTime.Now;
                    var result = dbAchievement.Save();
                    if (result)
                    {
                        var messageBuilder = game.messageBuilder;
                        var message = messageBuilder.GetText("AchievementUnlocked");
                        var achievementText = "<b>" + achievement.Name + "</b>";
                        var replaces = new Dictionary<string, object> { { "name", messageBuilder.FormatName(user) }, { "achievement", achievementText } };
                        messageBuilder
                            .Text(messageBuilder.Format(message, replaces), false)
                            .SendPublic(game.gameChannel);
                    }
                    return result;
                }
            }
            return false;
        }

        public void apply()
        {
            foreach (var achievementPair in achievements)
            {
                addInstantly(achievementPair.Item1, achievementPair.Item2.Id);
            }
            achievements.Clear();
        }

        public IList<DB.Achievement> getAchievements(UserWrapper user)
        {
            return DB.Achievement.findUserAchievements(user.Id);
        }

        public string getAchievementsAsString(UserWrapper user)
        {
            var builder = new System.Text.StringBuilder();
            var achievements = DB.Achievement.findUserAchievements(user.Id);
            builder.AppendLine("<b><u>Достижения:</u></b>");
            foreach (var dbAchievement in achievements)
            {
                Achievement achievement;
                if (allowedAchievements.TryGetValue(dbAchievement.achievementId, out achievement))
                {
                    builder.AppendLine($"<b>{achievement.Name}</b> ({achievement.Description}) - заработано {dbAchievement.achievedAt.ToString()}");
                }
            }
            return builder.ToString();
        }
    }
}
