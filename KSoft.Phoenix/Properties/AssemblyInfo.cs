using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("1a1f287c-66cc-480c-9fc2-54a4e0f11c47")]

[assembly: InternalsVisibleTo("Test.KSoft.Phoenix")]
[assembly: SuppressMessage("Design",
	"CA1033:Interface methods should be callable by child types",
	Justification = "Explicit interface implementations intentionally preserve collection and database model contracts")]
[assembly: SuppressMessage("Design",
	"CA1041:Provide ObsoleteAttribute message",
	Justification = "Legacy game-format enum values have no uniform replacement")]
[assembly: SuppressMessage("Design",
	"CA1062:Validate arguments of public methods",
	Justification = "Public serialization and stream APIs require caller-established non-null stream/model contracts")]