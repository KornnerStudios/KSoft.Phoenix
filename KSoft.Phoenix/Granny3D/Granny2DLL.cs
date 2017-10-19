using System;
using System.Runtime.InteropServices;
using Vector3 = SlimMath.Vector3;
using Vector4 = SlimMath.Vector4;
using granny_matrix_4x4 = SlimMath.Matrix;

namespace KSoft.Granny3D
{
	public static class Granny2DLL
	{
		const string kDllName = @"Granny3D\Granny2.dll";
		const CharSet kCharSet = CharSet.Ansi;

		[DllImport(kDllName, CharSet=kCharSet)]
		public static extern bool GrannyFindBoneByName(TPtr<granny_skeleton> Skeleton, string BoneName, out int BoneIndex);

		[DllImport(kDllName)]
		public static extern void GrannyBuildCompositeTransform4x4(
			[In] ref granny_transform transform,
			out granny_matrix_4x4 matrix4x4);

		[DllImport(kDllName)]
		public static extern TPtr<granny_texture> GrannyGetMaterialTextureByType(
			TPtr<granny_material> material,
			granny_material_texture_type channel);

		[DllImport(kDllName)]
		public static extern void GrannyConvertSingleObject(
			TPtr<granny_data_type_definition> SourceType,
			IntPtr SourceObject,
			TPtr<granny_data_type_definition> DestType,
			IntPtr DestObject);

		[DllImport(kDllName)]
		public static extern int GrannyGetTypeTableCount(IntPtr /* granny_data_type_definition** */ TypeTable);

		[DllImport(kDllName)]
		public static extern void GrannyBuildSkeletonRelativeTransforms(
			int SourceTransformStride,
			[In] granny_transform[] SourceTransforms,
			int SourceParentStride,
			[In] int[] SourceParents,
			int Count,
			int ResultStride,
			[Out] granny_transform[] Results);

		#region granny_file
		[DllImport(kDllName)]
		public static extern IntPtr /* granny_file* */ GrannyReadEntireFile(string filename);
		[DllImport(kDllName)]
		public static extern void GrannyFreeFile(IntPtr /* granny_file* */ File);
		[DllImport(kDllName)]
		public static extern void GrannyFreeFileSection(IntPtr /* granny_file* */ File, int SectionIndex);
		[DllImport(kDllName)]
		public static extern TPtr<granny_file_info> GrannyGetFileInfo(IntPtr /* granny_file* */File);
		#endregion

		#region granny_file_info
		[DllImport(kDllName)]
		public static extern bool GrannyComputeBasisConversion(
			[In] TPtr<granny_file_info> FileInfo,
			float DesiredUnitsPerMeter,
			[In] ref Vector3 DesiredOrigin3,
			[In] ref Vector3 DesiredRight3,
			[In] ref Vector3 DesiredUp3,
			[In] ref Vector3 DesiredBack3,
			out Vector3 ResultAffine3,
			out granny_matrix_3x3 ResultLinear3x3,
			out granny_matrix_3x3 ResultInverseLinear3x3);

		[DllImport(kDllName)]
		public static extern void GrannyTransformFile(
			TPtr<granny_file_info> FileInfo,
			[In] ref Vector3 Affine3,
			[In] ref granny_matrix_3x3 Linear3x3,
			[In] ref granny_matrix_3x3 InverseLinear3x3,
			float AffineTolerance,
			float LinearTolerance,
			granny_transform_file_flags Flags);
		#endregion

		#region granny_mesh
		[DllImport(kDllName)]
		public static extern TPtr<granny_tri_material_group> GrannyGetMeshTriangleGroups(TPtr<granny_mesh> mesh);
		[DllImport(kDllName)]
		public static extern bool GrannyMeshIsRigid(TPtr<granny_mesh> Mesh);
		[DllImport(kDllName)]
		public static extern int GrannyGetMeshIndexCount(TPtr<granny_mesh> Mesh);
		[DllImport(kDllName)]
		public static extern void GrannyCopyMeshIndices(TPtr<granny_mesh> mesh, int bytesPerIndex, IntPtr dstIndexData);
		[DllImport(kDllName)]
		public static extern TPtr<granny_data_type_definition> GrannyGetMeshVertexType(TPtr<granny_mesh> mesh);
		[DllImport(kDllName)]
		public static extern int GrannyGetMeshVertexCount(TPtr<granny_mesh> mesh);
		[DllImport(kDllName)]
		public static extern IntPtr GrannyGetMeshVertices(TPtr<granny_mesh> Mesh);
		[DllImport(kDllName)]
		public static extern void GrannyCopyMeshVertices(TPtr<granny_mesh> mesh, TPtr<granny_data_type_definition> vertType, IntPtr dstVertexData);
		[DllImport(kDllName)]
		public static extern int GrannyGetMeshTriangleCount(TPtr<granny_mesh> mesh);
		[DllImport(kDllName)]
		public static extern int GrannyGetMeshTriangleGroupCount(TPtr<granny_mesh> mesh);
		#endregion

		#region granny_model_instance
		[DllImport(kDllName)]
		public static extern TPtr<granny_model_instance> GrannyInstantiateModel(TPtr<granny_model> Model);
		[DllImport(kDllName)]
		public static extern void GrannyFreeModelInstance(TPtr<granny_model_instance> ModelInstance);
		[DllImport(kDllName)]
		public static extern TPtr<granny_model_control_binding> GrannyModelControlsBegin(TPtr<granny_model_instance> Model);
		[DllImport(kDllName)]
		public static extern TPtr<granny_model_control_binding> GrannyModelControlsEnd(TPtr<granny_model_instance> Model);
		[DllImport(kDllName)]
		public static extern TPtr<granny_model_control_binding> GrannyModelControlsNext(TPtr<granny_model_control_binding> Binding);

		[DllImport(kDllName)]
		public static extern TPtr<granny_skeleton> GrannyGetSourceSkeleton(TPtr<granny_model_instance> Model);

		[DllImport(kDllName)]
		public static extern void GrannySetModelClock(TPtr<granny_model_instance> ModelInstance, float NewClock);

		[DllImport(kDllName)]
		public static extern void GrannyUpdateModelMatrix(
			TPtr<granny_model_instance> ModelInstance,									
			float SecondsElapsed,			
			[In] ref granny_matrix_4x4 ModelMatrix4x4,					
			out granny_matrix_4x4 DestMatrix4x4,													
			bool Inverse);

		[DllImport(kDllName)]
		public static extern void GrannySampleModelAnimations(
			TPtr<granny_model_instance> ModelInstance,
			int FirstBone,
			int BoneCount,
			TPtr<granny_local_pose> Result);

		[DllImport(kDllName)]
		public static extern void GrannyFreeCompletedModelControls(TPtr<granny_model_instance> ModelInstance);
		#endregion

		#region granny_control
		[DllImport(kDllName)]
		public static extern TPtr<granny_control> GrannyPlayControlledAnimation(
			float StartTime,
			TPtr<granny_animation> Animation,
			TPtr<granny_model_instance> Model);
		[DllImport(kDllName)]
		public static extern void GrannyFreeControl(TPtr<granny_control> Control);
		[DllImport(kDllName)]
		public static extern void GrannySetControlLoopCount(TPtr<granny_control> Control, int LoopCount);
		[DllImport(kDllName)]
		public static extern void GrannyFreeControlOnceUnused(TPtr<granny_control> Control);
		[DllImport(kDllName)]
		public static extern TPtr<granny_control> GrannyGetControlFromBinding(TPtr<granny_model_control_binding> Binding);
		[DllImport(kDllName)]
		public static extern float GrannyEaseControlOut(TPtr<granny_control> Control, float Duration);
		[DllImport(kDllName)]
		public static extern void GrannyCompleteControlAt(TPtr<granny_control> Control, float AtSeconds);
		[DllImport(kDllName)]
		public static extern IntPtr /* void** */ GrannyGetControlUserDataArray(TPtr<granny_control> Control);
		#endregion

		#region granny_world_pose
		[DllImport(kDllName)]
		public static extern TPtr<granny_world_pose> GrannyNewWorldPose(int BoneCount);
		[DllImport(kDllName)]
		public static extern void GrannyFreeWorldPose(TPtr<granny_world_pose> WorldPose);
		[DllImport(kDllName)]
		public static extern TPtr<granny_matrix_4x4> GrannyGetWorldPoseComposite4x4Array(TPtr<granny_world_pose> WorldPose);
		[DllImport(kDllName)]
		public static extern void GrannyBuildWorldPose(
			TPtr<granny_skeleton> Skeleton,
			int FirstBone,
			int BoneCount,
			TPtr<granny_local_pose> LocalPose,
			[In] ref granny_matrix_4x4 Offset4x4,
			TPtr<granny_world_pose> Result);
		[DllImport(kDllName)]
		public static extern TPtr<granny_matrix_4x4> GrannyGetWorldPose4x4(TPtr<granny_world_pose> WorldPose, int BoneIndex);
		#endregion

		#region granny_local_pose
		[DllImport(kDllName)]
		public static extern TPtr<granny_local_pose> GrannyNewLocalPose(int BoneCount);
		[DllImport(kDllName)]
		public static extern void GrannyFreeLocalPose(TPtr<granny_local_pose> pose);
		[DllImport(kDllName)]
		public static extern void GrannyGetWorldMatrixFromLocalPose(
			[In] TPtr<granny_skeleton> Skeleton,
			int BoneIndex,
			[In] TPtr<granny_local_pose> LocalPose,
			[In] ref granny_matrix_4x4 Offset4x4,
			out granny_matrix_4x4 Result4x4,
			[In] int[] SparseBoneArray,
			[In] int[] SparseBoneArrayReverse);
		#endregion

		#region granny_mesh_binding
		[DllImport(kDllName)]
		public static extern TPtr<granny_mesh_binding> GrannyNewMeshBinding(
			TPtr<granny_mesh> Mesh,
			TPtr<granny_skeleton> FromSkeleton,
			TPtr<granny_skeleton> ToSkeleton);
		[DllImport(kDllName)]
		public static extern void GrannyFreeMeshBinding(TPtr<granny_mesh_binding> binding);
		[DllImport(kDllName)]
		public static extern TPtr<int> GrannyGetMeshBindingToBoneIndices(TPtr<granny_mesh_binding> Binding);
		#endregion

		#region granny_mesh_deformer
		[DllImport(kDllName)]
		public static extern TPtr<granny_mesh_deformer> GrannyNewMeshDeformer(
			TPtr<granny_data_type_definition> InputVertexLayout,
			TPtr<granny_data_type_definition> OutputVertexLayout,
			granny_deformation_type DeformationType,									  
			granny_deformer_tail_flags TailFlag);
		[DllImport(kDllName)]
		public static extern void GrannyFreeMeshDeformer(TPtr<granny_mesh_deformer> Deformer);
		[DllImport(kDllName)]
		public static extern void GrannyDeformVertices(
			TPtr<granny_mesh_deformer> Deformer,
			[In] int[] MatrixIndices,
			[In] ref granny_matrix_4x4 MatrixBuffer4x4,
			int VertexCount,
			IntPtr SourceVertices,
			IntPtr DestVertices);
		#endregion
	};
}