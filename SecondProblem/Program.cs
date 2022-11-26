using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading;
using System.Collections;

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

    static Dictionary<string, Dictionary<string, List<KeyValuePair<int, string>>>> userValuesWithTime = new Dictionary<string, Dictionary<string, List<KeyValuePair<int, string>>>>();
    static Dictionary<string, Dictionary<string, string>> output = new Dictionary<string, Dictionary<string, string>>();

    static List<Thread> threads = new List<Thread>();

    static void Main(string[] args)
    {
      try
      {
        string path = args[0];
        StreamReader sr = File.OpenText(path);
        string s;

        while ((s = sr.ReadLine()) != null)
        {
          //   Thread th = new Thread(() => processIncomingUpdate(s));
          //   threads.Add(th);
          //   th.Start();
          processIncomingUpdate(s);
        }

        // foreach (Thread thread in threads)
        // {
        //   thread.Join();
        // }

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

      lock (userValuesWithTime)
      {
        if (!userValuesWithTime.ContainsKey(incomingUpdate.user))
        {
          userValuesWithTime[incomingUpdate.user] = new Dictionary<string, List<KeyValuePair<int, string>>>();
        }
        if (!userValuesWithTime[incomingUpdate.user].ContainsKey(incomingValues.Key))
        {
          userValuesWithTime[incomingUpdate.user][incomingValues.Key] = new List<KeyValuePair<int, string>>();
        }
        userValuesWithTime[incomingUpdate.user][incomingValues.Key].Add(new KeyValuePair<int, string>(incomingUpdate.timestamp, incomingValues.Value));
      }
    }

    public static void printOutput()
    {
      foreach (KeyValuePair<string, Dictionary<string, List<KeyValuePair<int, string>>>> kvp in userValuesWithTime)
      {
        output[kvp.Key] = new Dictionary<string, string>();
        foreach (KeyValuePair<string, List<KeyValuePair<int, string>>> kvp2 in kvp.Value)
        {
          int maxTimeStamp = 0;
          foreach (KeyValuePair<int, string> kvp3 in kvp2.Value)
          {
            if (kvp3.Key > maxTimeStamp)
            {
              maxTimeStamp = kvp3.Key;
            }
          }

          foreach (KeyValuePair<int, string> kvp3 in kvp2.Value)
          {
            if (kvp3.Key == maxTimeStamp)
            {
              output[kvp.Key][kvp2.Key] = kvp3.Value;
            }
          }
        }
      }

      Console.WriteLine(JsonSerializer.Serialize(output));
    }
  }
}