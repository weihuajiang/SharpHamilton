using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STARLine
{
    class StepErrors
    {
        Dictionary<int, ModuleErrors> Errors = new Dictionary<int, ModuleErrors>();
        public void ClearErrorForTask(int task)
        {
            if (Errors.ContainsKey(task))
                Errors.Remove(task);
        }
        public void SetErrorForTask(int task, ModuleErrors err)
        {
            Errors.Add(task, err);
        }
        public ModuleErrors GetErrorTask(int task)
        {
            if (Errors.ContainsKey(task))
                return Errors[task];
            return null;
        }
    }
}
