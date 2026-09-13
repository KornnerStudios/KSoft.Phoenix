using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Phoenix.HaloWars.Test;

[TestClass]
public sealed class ModManifestFileCachedNotificationTests
{
	[TestMethod]
	public void FilePathNotifications_PreserveOrderAndReuseCachedArgs()
	{
		var model = new ModManifestFile
		{
			FilePath = @"relative\initial\ModManifest.txt",
		};
		var propertyNames = new List<string?>();
		var eventArgs = new List<PropertyChangedEventArgs>();
		model.PropertyChanged += (_, args) =>
		{
			propertyNames.Add(args.PropertyName);
			eventArgs.Add(args);
		};

		model.FilePath = @"relative\first\ModManifest.txt";
		model.FilePath = @"relative\second\ModManifest.txt";

		CollectionAssert.AreEqual(
			new[] {
				nameof(ModManifestFile.FilePath),
				nameof(ModManifestFile.ContainingFolder),
				nameof(ModManifestFile.DisplayTitle),
				nameof(ModManifestFile.FilePath),
				nameof(ModManifestFile.ContainingFolder),
				nameof(ModManifestFile.DisplayTitle),
			},
			propertyNames);
		Assert.AreSame(eventArgs[0], eventArgs[3]);
		Assert.AreSame(eventArgs[1], eventArgs[4]);
		Assert.AreSame(eventArgs[2], eventArgs[5]);
	}

	[TestMethod]
	public void FilePathNotificationException_PreservesAssignmentAndStopsCascade()
	{
		var model = new ModManifestFile
		{
			FilePath = @"relative\initial\ModManifest.txt",
		};
		var expected = new InvalidOperationException("FilePath notification failed.");
		var propertyNames = new List<string?>();
		model.PropertyChanged += (_, args) =>
		{
			propertyNames.Add(args.PropertyName);
			if (args.PropertyName == nameof(ModManifestFile.FilePath))
			{
				throw expected;
			}
		};

		var exception = Assert.ThrowsExactly<InvalidOperationException>(
			() => model.FilePath = @"relative\throws\ModManifest.txt");

		Assert.AreSame(expected, exception);
		Assert.AreEqual(@"relative\throws\ModManifest.txt", model.FilePath);
		CollectionAssert.AreEqual(
			new[] { nameof(ModManifestFile.FilePath) },
			propertyNames);
	}
}
