using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SecondProblem
{
  public class Update
  {
    public string type { get; set; }
    public string user { get; set; }
    public int timestamp { get; set; }
    public Dictionary<string, string> values { get; set; }
  }
  class Program
  {

    static ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentStack<KeyValuePair<int, string>>>> userValuesWithTime = new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentStack<KeyValuePair<int, string>>>>();
    static Dictionary<string, Dictionary<string, string>> output = new Dictionary<string, Dictionary<string, string>>();
    static List<Thread> threads = new List<Thread>();
    static List<Task> tasks = new List<Task>();

    static void Main(string[] args)
    {
      try
      {
        string path = args[0];
        StreamReader sr = File.OpenText(path);
        string s;

        while ((s = sr.ReadLine()) != null)
        {
          var JsonLine = s;
          tasks.Add(Task.Factory.StartNew(() => processIncomingUpdate(JsonLine)));
        }

        Task.WaitAll(tasks.ToArray());
        printOutput();
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }
    }

    public static void processIncomingUpdate(string json)
    {
      Update incomingUpdate = JsonSerializer.Deserialize<Update>(json);
      KeyValuePair<string, string> incomingValues = new KeyValuePair<string, string>();
      foreach (KeyValuePair<string, string> kvp in incomingUpdate.values)
      {
        incomingValues = kvp;
      }

      if (!userValuesWithTime.ContainsKey(incomingUpdate.user))
      {
        userValuesWithTime.TryAdd(incomingUpdate.user, new ConcurrentDictionary<string, ConcurrentStack<KeyValuePair<int, string>>>());
      }
      if (!userValuesWithTime[incomingUpdate.user].ContainsKey(incomingValues.Key))
      {
        userValuesWithTime[incomingUpdate.user].TryAdd(incomingValues.Key, new ConcurrentStack<KeyValuePair<int, string>>());
      }
      userValuesWithTime[incomingUpdate.user][incomingValues.Key].Push(new KeyValuePair<int, string>(incomingUpdate.timestamp, incomingValues.Value));
    }

    public static void printOutput()
    {
      foreach (KeyValuePair<string, ConcurrentDictionary<string, ConcurrentStack<KeyValuePair<int, string>>>> kvp in userValuesWithTime)
      {
        output[kvp.Key] = new Dictionary<string, string>();
        foreach (KeyValuePair<string, ConcurrentStack<KeyValuePair<int, string>>> kvp2 in kvp.Value)
        {
          int maxTimeStamp = 0;
          foreach (KeyValuePair<int, string> kvp3 in kvp2.Value.ToArray())
          {
            if (kvp3.Key > maxTimeStamp)
            {
              maxTimeStamp = kvp3.Key;
            }
          }
          foreach (KeyValuePair<int, string> kvp3 in kvp2.Value.ToArray())
          {
            if (kvp3.Key == maxTimeStamp)
            {
              output[kvp.Key][kvp2.Key] = kvp3.Value;
            }
          }
        }
      }

      foreach (KeyValuePair<string, Dictionary<string, string>> kvp in output)
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(kvp.Key + ":");
        foreach (KeyValuePair<string, string> kvp2 in kvp.Value)
        {
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine("    " + kvp2.Key + ": " + kvp2.Value);
        }
      }
    }
  }
}