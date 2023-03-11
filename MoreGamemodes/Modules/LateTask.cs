using System;
using System.Collections.Generic;

namespace MoreGamemodes
{
    class LateTask
    {
        public string name;
        public float timer;
        public Action action;
        public bool deleteOnMeeting;
        public static List<LateTask> Tasks = new();
        public bool Run(float deltaTime)
        {
            timer -= deltaTime;
            if (timer <= 0)
            {
                action();
                return true;
            }
            return false;
        }
        public LateTask(Action action, float time, string name = "No Name Task", bool deleteOnMeeting = false)
        {
            this.action = action;
            this.timer = time;
            this.name = name;
            this.deleteOnMeeting = deleteOnMeeting;
            Tasks.Add(this);
        }
        public static void Update(float deltaTime)
        {
            var TasksToRemove = new List<LateTask>();
            for (int i = 0; i < Tasks.Count; i++)
            {
                var task = Tasks[i];
                try
                {
                    if (task.Run(deltaTime))
                    {
                        TasksToRemove.Add(task);
                    }
                    else if (task.deleteOnMeeting && Main.IsMeeting)
                    {
                        TasksToRemove.Add(task);
                    }
                }
                catch (Exception ex)
                {
                    TasksToRemove.Add(task);
                }
            }
            TasksToRemove.ForEach(task => Tasks.Remove(task));
        }
    }
}