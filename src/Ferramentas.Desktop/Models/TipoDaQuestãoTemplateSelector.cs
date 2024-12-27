using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Ferramentas.Desktop.Models.Enumerados;

namespace Ferramentas.Desktop.Models;

public class TipoDaQuestãoTemplateSelector : IDataTemplate
{
    public IDataTemplate? TextoTemplate { get; set; }
    public IDataTemplate? EscolhaÚnicaTemplate { get; set; }

    public Control Build(object? data)
    {
        if (data is QuestãoDoPullRequest questão)
        {
            return questão.TipoDaQuestão switch
                {
                    TipoDaQuestão.Texto        => TextoTemplate?.Build(data),
                    TipoDaQuestão.EscolhaÚnica => EscolhaÚnicaTemplate?.Build(data),
                    _                          => throw new InvalidOperationException("Tipo desconhecido.")
                }
             ?? throw new InvalidOperationException("Template não encontrado.");
        }

        throw new InvalidOperationException("Data não é do tipo esperado.");
    }

    public bool Match(object? data) =>
        data is QuestãoDoPullRequest;
}