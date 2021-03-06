﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using DTOProject.DTO;



namespace MyLib
{
    public class Utils
    {
        


        /// <summary>
        /// Convert an object to a byte array
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Convert a byte array to an Object
        /// </summary>
        /// <param name="arrBytes"></param>
        /// <returns></returns>
        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }


        /// <summary>
        /// Get Data question from file 
        /// </summary>
        /// <param name="pathFile"></param>
        /// <param name="listGameShow"></param>
        public static void getScheduleFromFile(string pathFile, ref List<GameShow> listGameShow)
        {
            StreamReader sr = new StreamReader(pathFile);
            string line = null;
            GameShow game = null;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith("!"))
                {
                    game = new GameShow();
                    game.ID = line.Substring(1);
                }
                else if (line.StartsWith("@"))
                {
                    game.Name = line.Substring(1);
                }
                else if (line.StartsWith("#"))
                {
                    game.StartTime =  DateTime.Parse(line.Substring(1));
                }
                else if (line.StartsWith("$"))
                {
                    game.EndTime = DateTime.Parse(line.Substring(1));
                }
                else if (line.StartsWith("%"))
                {
                    game.NumberPlayer = int.Parse(line.Substring(1));
                    listGameShow.Add(game);
                }
            }
            sr.Close();
            sortSchedule(listGameShow,2);
        }

        public static void sortSchedule(List<GameShow> listGameShow,int numColum)
        {
            switch (numColum)
            {
                case 0:
                    listGameShow.Sort((a, b) => a.ID.CompareTo(b.ID));
                    break;
                case 1:
                    listGameShow.Sort((a, b) => a.Name.CompareTo(b.Name));
                    break;
                case 2:
                    listGameShow.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
                    break;
                case 3:
                    listGameShow.Sort((a, b) => a.EndTime.CompareTo(b.EndTime));
                    break;
                case 4:
                    listGameShow.Sort((a, b) => a.NumberPlayer.CompareTo(b.NumberPlayer));
                    break;
            }
        }

        public static void sortQuestion(List<Question> listQuestion, int numColum)
        {
            switch (numColum)
            {
                case 0:
                    listQuestion.Sort((a, b) => a.Id.CompareTo(b.Id));
                    break;
                case 1:
                    listQuestion.Sort((a, b) => a.Content.CompareTo(b.Content));
                    break;
                case 2:
                    listQuestion.Sort((a, b) => a.AnswerCorrect.CompareTo(b.AnswerCorrect));
                    break;
                case 3:
                    listQuestion.Sort((a, b) => a.TopicInfor.CompareTo(b.TopicInfor));
                    break;
            }
        }


        /// <summary>
        /// Find nearest game in Broadcast Schedule
        /// </summary>
        /// <param name="listGameShow"></param>
        /// <returns></returns>
        public static GameShow findNearestGame(List<GameShow> listGameShow) 
        {
            DateTime timeNow = DateTime.Now;
            GameShow nearestGame = null;
            listGameShow.ForEach(game =>
            {
                //Check whether the game has a valid broadcast time and is less than the min time
                if (timeNow < game.StartTime) 
                {
                    if(nearestGame == null) nearestGame = game;
                    if (game.StartTime.Subtract(timeNow).TotalMilliseconds < nearestGame.StartTime.Subtract(timeNow).TotalMilliseconds)
                    {
                        nearestGame = game;
                    }
                }
            });
            return nearestGame;
        }

        /// <summary>
        /// Find the game is broadcasting in Broadcast Schedule
        /// </summary>
        /// <param name="listGameShow"></param>
        /// <returns></returns>
        public static GameShow findGameIsBroadcasting(List<GameShow> listGameShow) 
        {
            DateTime timeNow = DateTime.Now;
            GameShow gameIsBroadcasting = null;
            listGameShow.ForEach(game =>
            {
                //Check whether the game has a valid broadcast time and is less than the min time
                if (timeNow > game.StartTime && timeNow < game.EndTime)
                {
                    if (gameIsBroadcasting == null) gameIsBroadcasting = game;
                    if (game.StartTime.Subtract(timeNow).TotalMilliseconds > gameIsBroadcasting.StartTime.Subtract(timeNow).TotalMilliseconds)
                    {
                        gameIsBroadcasting = game;
                    }
                }
            });
            return gameIsBroadcasting;
        }

        /// <summary>
        /// Calculate the time distance with the nearest game
        /// </summary>
        /// <param name="nearestGame"></param>
        /// <returns></returns>
        public static int calcWaitingTime(GameShow nearestGame)
        {
            return (int)(nearestGame.StartTime.Subtract(DateTime.Now).TotalSeconds);
            
        }

        /// <summary>
        /// Update player score
        /// </summary>
        /// <param name="answer">player's answer</param>
        /// <param name="ID">player's id</param>
        /// <param name="question">Current question</param>
        /// <param name="listScore"></param>
        public static void updateListScores(string answer, string ID, Question question, Dictionary<string, int> listScore)
        {
            if (answer == "A" && question.listAnswer[3] == question.AnswerCorrect)
            {
                listScore[ID]++;
            }
            else if (answer == "B" && question.listAnswer[2] == question.AnswerCorrect)
            {
                listScore[ID]++;

            }
            else if (answer == "C" && question.listAnswer[1] == question.AnswerCorrect)
            {
                listScore[ID]++;
            }
            else if (answer == "D" && question.listAnswer[0] == question.AnswerCorrect)
            {
                listScore[ID]++;
            }
        }

        public static Dictionary<string, int> Rank(Dictionary<string, int> listScore)
        {
            return listScore.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

        }

    }
}
