/* -------- Collection aliases */

/* -------- Math aliases */

global using BMatrix = System.Numerics.Matrix4x4;
global using BVec2 = System.Numerics.Vector2;
global using BVector = System.Numerics.Vector4;

/* -------- Proto aliases*/

//[Meta.BDamageTypeReference]
global using BDamageTypeID = System.Int32;

//[Meta.BProtoObjectReference]
global using BProtoObjectID = System.Int32; // a proto object (objects.xml) or object type (object_types.xml)

//[Meta.ObjectTypeReference]
global using BObjectTypeID = System.Int32;

//[Meta.BProtoPowerReference]
global using BProtoPowerID = System.Int32;

//[Meta.BProtoActionReference]
global using BProtoActionID = System.Int32;

//[Meta.BProtoSquadReference]
global using BProtoSquadID = System.Int32;

//[Meta.BProtoTechReference]
global using BProtoTechID = System.Int32;

// #TODO these should all be a BObjectTypeID. Need to #REMOVE BProtoUnitID and UnitReference
//[Meta.UnitReference]
global using BProtoUnitID = System.Int32; // object type or proto unit

/* -------- Runtime aliases */

global using BEntityID = System.Int32;
