namespace Alarm.Core
{
    public partial class Application
    {
        readonly Stack<Action<int>> onExit = [];
        readonly List<Task> mainTasks = [];

        public void WaitAllTasks()
        {
            Task.WaitAll(mainTasks);
        }

        public void StartMainTasks()
        {
            foreach (var task in mainTasks)
            {
                task.Start();
            }
        }

        public void AddExitAction(Action<int> action)
        {
            onExit.Push(action);
        }

        public void AddMainTask(Task task)
        {
            mainTasks.Add(task);
        }
    }
}
