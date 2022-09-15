﻿using Bogus;using IS.Managed;
using k8s;
using Spectre.Console;

AnsiConsole.MarkupLine(
    $"[link=https://github.com/vrhovnik/infobip-shift-2022-demos]Demo for working with Managed C# Kubernetes Api[/]!");
AnsiConsole.WriteLine("Loading from:");
AnsiConsole.Write(new TextPath(@"C:\Users\bovrhovn\.kube\config")
    .RootStyle(new Style(foreground: Color.Red))
    .SeparatorStyle(new Style(foreground: Color.Green))
    .StemStyle(new Style(foreground: Color.Blue))
    .LeafStyle(new Style(foreground: Color.Yellow)));

var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
IKubernetes client = new Kubernetes(config);
AnsiConsole.WriteLine($"Listening to API master at {config.Host}");

HorizontalRule("01 - Namespace operations and demos");

var namespaceOps = new NamespaceOperations(client);
await namespaceOps.ListAllNamespacesAsync();

var nsName = AnsiConsole.Ask<string>("What [green]namespace[/] would you like to create?");
await namespaceOps.CreateNamespaceAsync(nsName,
    new Dictionary<string, string> { { "app", "cli" }, { "conf", "InfobipShift" }, { "type", "ns" } });

await namespaceOps.ListAllNamespacesAsync();

if (!AnsiConsole.Confirm($"Delete namespace {nsName}?")) await namespaceOps.DeleteNamespacesAsync(nsName);

HorizontalRule("02 - workloads operations");

 var workloadOps = new WorkloadOperations(client);

var namespaceList = await namespaceOps.GetNamespacesAsync();
var namespaceToCheckPods = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Pick [green]namespace[/] to get pods?")
        .PageSize(10)
        .MoreChoicesText("[grey](Move up and down to reveal more namespaces)[/]")
        .AddChoices(namespaceList));

await workloadOps.OutputPodsAsync(namespaceToCheckPods);

var podImage = AnsiConsole.Ask<string>("What [green]image[/] would you like to use for creating the pod?");

var podName = new Faker().Hacker.Abbreviation();

await workloadOps.CreatePodAsync(podName, podImage,
    new Dictionary<string, string> { { "app", "cli" }, { "conf", "InfobipShift" }, { "type", "pods" } },
    namespaceToCheckPods);

await workloadOps.OutputPodsAsync(namespaceToCheckPods);

HorizontalRule("03 - use watch option");

await workloadOps.GetPodsWithWatchEnabledAsync(namespaceToCheckPods);
//open new PWSH terminal and delete the pod kubectl delete pod nameofthepod -n namespacetocheckpods
//return back here and press CTRL + C to continue

HorizontalRule("04 - load yaml and do modifications");

await workloadOps.LoadYamlOutputDataAsync();



void HorizontalRule(string title)
{
    AnsiConsole.WriteLine();
    AnsiConsole.Write(new Rule($"[white bold]{title}[/]").RuleStyle("grey").LeftAligned());
    AnsiConsole.WriteLine();
}