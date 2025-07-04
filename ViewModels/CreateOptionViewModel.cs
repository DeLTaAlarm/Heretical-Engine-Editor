using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HereticalEngineEditor.ViewModels;
public class CreateOptionViewModel
{
    public string Name { get; }
    public ICommand Command { get; }

    public CreateOptionViewModel(string name, ICommand command)
    {
        Name = name;
        Command = command;
    }
}
