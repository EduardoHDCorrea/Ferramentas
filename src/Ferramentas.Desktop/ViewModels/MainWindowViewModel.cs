using System;
using System.Collections.ObjectModel;
using System.IO;
using Ferramentas.Desktop.Models;

namespace Ferramentas.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private ModeloDoMarkdown questões = new ModeloDoMarkdown(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "modelo-markdown-pr.json")
    );

    public ObservableCollection<QuestãoDoPullRequest> Questões
    {
        get => questões.Questões;
        set => questões.Questões = value;
    }
}