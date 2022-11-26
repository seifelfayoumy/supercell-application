using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace FirstProblem
{

  public class CustomKeyValueStore
  {
    public string type { get; set; }
    public string user { get; set; }
    public string user1 { get; set; }
    public string user2 { get; set; }
    public int timestamp { get; set; }
    public Dictionary<string, string> values { get; set; }
  }

  public class Broadcast
  {
    public string[] broadcast { get; set; }
    public string user { get; set; }
    public int timestamp { get; set; }
    public Dictionary<string, string> values { get; set; }
  }

  class Program
  {
    static Dictionary<string, List<string>> friends = new Dictionary<string, List<string>>();

    static List<int> timestamps = new List<int>();
    static Dictionary<string, string> updatedValues = new Dictionary<string, string>();
    static void Main(string[] args)
    {
      try
      {
        string path = args[0];
        StreamReader sr = File.OpenText(path);
        {
          string s;
          while ((s = sr.ReadLine()) != null)
          {
            CustomKeyValueStore currentEvent = JsonSerializer.Deserialize<CustomKeyValueStore>(s);
            if (currentEvent.type.Equals("make_friends"))
            {
              addFriend(currentEvent.user1, currentEvent.user2);
              addFriend(currentEvent.user2, currentEvent.user1);
            }
            else if (currentEvent.type.Equals("del_friends"))
            {
              removeFriend(currentEvent.user1, currentEvent.user2);
              removeFriend(currentEvent.user2, currentEvent.user1);
            }
            else if (currentEvent.type.Equals("update"))
            {
              Dictionary<string, string> currentValues = getBroadcastValues(currentEvent.values, currentEvent.timestamp);
              if (currentValues.Count != 0 && currentEvent.user != null && currentEvent.user != "" && getFriends(currentEvent.user) != null && getFriends(currentEvent.user).Length != 0)
              {
                Broadcast newBroadcast = new Broadcast();
                newBroadcast.broadcast = getFriends(currentEvent.user);
                newBroadcast.user = currentEvent.user;
                newBroadcast.timestamp = currentEvent.timestamp;
                newBroadcast.values = currentValues;
                Console.WriteLine(JsonSerializer.Serialize(newBroadcast));
              }
            }
          }
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }

    }

    public static string[] getFriends(string user)
    {
      if (friends.ContainsKey(user))
      {
        return friends[user].ToArray();
      }
      return null;
    }
    public static void addFriend(string user, string friend)
    {
      if (friends.ContainsKey(user))
      {
        friends[user].Add(friend);
      }
      else
      {
        friends.Add(user, new List<string>());
        friends[user].Add(friend);
      }
    }

    public static void removeFriend(string user, string friend)
    {
      if (friends.ContainsKey(user))
      {
        friends[user].Remove(friend);
      }

    }

    public static Dictionary<string, string> getBroadcastValues(Dictionary<string, string> values, int timestamp)
    {
      Boolean newTimestamp = true;
      Dictionary<string, string> result = new Dictionary<string, string>();
      foreach (int stamp in timestamps)
      {
        if (timestamp <= stamp)
        {
          newTimestamp = false;
        }
      }
      foreach (KeyValuePair<string, string> valuePair in values)
      {
        if (newTimestamp)
        {
          result.Add(valuePair.Key, valuePair.Value);
          updatedValues[valuePair.Key] = valuePair.Value;
        }
        else
        {
          if (!updatedValues.ContainsKey(valuePair.Key))
          {
            result.Add(valuePair.Key, valuePair.Value);
            updatedValues.Add(valuePair.Key, valuePair.Value);
          }
          else if (!updatedValues[valuePair.Key].Equals(valuePair.Value))
          {
            updatedValues[valuePair.Key] = valuePair.Value;
            result.Add(valuePair.Key, valuePair.Value);
          }
        }
      }

      timestamps.Add(timestamp);
      return result;
    }
  }
}
