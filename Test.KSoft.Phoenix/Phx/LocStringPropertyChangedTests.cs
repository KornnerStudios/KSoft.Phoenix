using System.Collections.Generic;
using System.IO;
using KSoft.IO;
using KSoft.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KSoft.Phoenix.Phx.Test;

[TestClass]
public sealed class LocStringPropertyChangedTests
{
	[TestMethod]
	public void Setters_DifferentThenEqualValues_RaiseExpectedNotifications()
	{
		var model = new LocString(17);
		var receivedNames = new List<string?>();
		model.PropertyChanged += (sender, args) =>
		{
			Assert.AreSame(model, sender);
			receivedNames.Add(args.PropertyName);
		};

		model.Category = LocStringCategory.Code;
		model.Scenario = "scenario";
		model.IsSubtitle = true;
		model.IsUpdate = true;
		model.MouseKeyboardID = 23;
		model.OriginalID = "original";
		model.Text = "text";

		CollectionAssert.AreEqual(
			new[] {
				nameof(LocString.Category),
				nameof(LocString.Scenario),
				nameof(LocString.IsSubtitle),
				nameof(LocString.IsUpdate),
				nameof(LocString.MouseKeyboardID),
				nameof(LocString.OriginalID),
				nameof(LocString.Text),
			},
			receivedNames);

		model.Category = LocStringCategory.Code;
		model.Scenario = new string("scenario".ToCharArray());
		model.IsSubtitle = true;
		model.IsUpdate = true;
		model.MouseKeyboardID = 23;
		model.OriginalID = new string("original".ToCharArray());
		model.Text = new string("text".ToCharArray());

		Assert.HasCount(7, receivedNames);
	}

	[TestMethod]
	public void Serialize_RoundTripUsesBackingFieldsWithoutNotifications()
	{
		var expected = new LocString(42)
		{
			Category = LocStringCategory.Campaign,
			Scenario = "Arcadia",
			IsSubtitle = true,
			IsUpdate = true,
			MouseKeyboardID = 84,
			OriginalID = "40 and 41",
			Text = "Evacuate",
		};

		string xml;
		using (var writeStream = XmlElementStream.CreateForWrite("LocString"))
		{
			expected.Serialize(writeStream);
			xml = writeStream.Document.OuterXml;
		}

		var document = new XmlDocumentWithLocation
		{
			XmlResolver = null,
		};
		document.LoadXml(xml);

		var actual = new LocString();
		int notifications = 0;
		actual.PropertyChanged += (_, _) => notifications++;
		using (var readStream = new XmlElementStream(
			document,
			document.DocumentElement!,
			FileAccess.Read))
		{
			actual.Serialize(readStream);
		}

		Assert.AreEqual(0, notifications);
		Assert.AreEqual(expected.ID, actual.ID);
		Assert.AreEqual(expected.Category, actual.Category);
		Assert.AreEqual(expected.Scenario, actual.Scenario);
		Assert.AreEqual(expected.IsSubtitle, actual.IsSubtitle);
		Assert.AreEqual(expected.IsUpdate, actual.IsUpdate);
		Assert.AreEqual(expected.MouseKeyboardID, actual.MouseKeyboardID);
		Assert.AreEqual(expected.OriginalID, actual.OriginalID);
		Assert.AreEqual(expected.Text, actual.Text);

		using var roundTripStream = XmlElementStream.CreateForWrite("LocString");
		actual.Serialize(roundTripStream);
		Assert.AreEqual(xml, roundTripStream.Document.OuterXml);
	}
}
