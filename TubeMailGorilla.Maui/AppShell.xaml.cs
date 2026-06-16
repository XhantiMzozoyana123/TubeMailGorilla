using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TubeMailGorillaApplication.Interfaces;
using TubeMailGorillaDomain.Interfaces;

namespace TubeMailGorilla.Maui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
	}
}