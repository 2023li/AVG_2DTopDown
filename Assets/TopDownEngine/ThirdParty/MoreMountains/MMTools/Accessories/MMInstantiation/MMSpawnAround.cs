using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// This class is used to describe spawn properties, to be used by the MMSpawnAround class.
	/// It's meant to be exposed and used by classes that are designed to spawn objects, typically loot systems 
	/// </summary>
	[System.Serializable]
	public class MMSpawnAroundProperties
	{
		/// the possible shapes objects can be spawned within
		public enum MMSpawnAroundShapes { Sphere, Cube }
       
        [Header("Shape")]
        /// the shape within which objects should spawn
        [MMLabel("生成形状")]
        [Tooltip("生成对象时使用的区域形状")]
        public MMSpawnAroundShapes Shape = MMSpawnAroundShapes.Sphere;

        [Header("Position")]
        /// the minimum distance to the origin of the spawn at which objects can be spawned
        [MMLabel("最小球体半径")]
        [Tooltip("生成对象距离原点的最小球体半径")]
        [MMEnumCondition("Shape", (int)MMSpawnAroundShapes.Sphere)]
        public float MinimumSphereRadius = 1f;

        /// the maximum distance to the origin of the spawn at which objects can be spawned
        [MMLabel("最大球体半径")]
        [Tooltip("生成对象距离原点的最大球体半径")]
        [MMEnumCondition("Shape", (int)MMSpawnAroundShapes.Sphere)]
        public float MaximumSphereRadius = 2f;

        /// the minimum size of the cube's base
        [MMLabel("最小立方体尺寸")]
        [Tooltip("立方体生成区域的最小基底尺寸")]
        [MMEnumCondition("Shape", (int)MMSpawnAroundShapes.Cube)]
        public Vector3 MinimumCubeBaseSize = Vector3.one;

        /// the maximum size of the cube's base
        [MMLabel("最大立方体尺寸")]
        [Tooltip("立方体生成区域的最大基底尺寸")]
        [MMEnumCondition("Shape", (int)MMSpawnAroundShapes.Cube)]
        public Vector3 MaximumCubeBaseSize = new Vector3(2f, 2f, 2f);

        [Header("Plane")]
        /// if this is true, spawn will be constrained to the plane defined by the NormalToSpawnPlane property
        [MMLabel("强制平面生成")]
        [Tooltip("是否将生成限制在指定法线方向的平面")]
        public bool ForcePlane = true;

        /// a Vector3 that specifies the normal to the plane you want to spawn objects on
        [MMLabel("平面法线")]
        [Tooltip("生成平面的法线方向（如x/z平面的法线是y轴(0,1,0)")]
        public Vector3 NormalToSpawnPlane = Vector3.up;

        [Header("NormalAxisOffset")]
        /// the minimum offset to apply on the normal axis
        [MMLabel("法线轴最小偏移")]
        [Tooltip("沿法线轴方向的最小位移量")]
        public float MinimumNormalAxisOffset = 0f;

        /// the maximum offset to apply on the normal axis
        [MMLabel("法线轴最大偏移")]
        [Tooltip("沿法线轴方向的最大位移量")]
        public float MaximumNormalAxisOffset = 0f;

        [Header("NormalAxisOffsetCurve")]
        /// whether or not to use a curve to offset the object's spawn position along the spawn plane
        [MMLabel("使用偏移曲线")]
        [Tooltip("是否使用曲线调整沿法线轴的偏移量")]
        public bool UseNormalAxisOffsetCurve = false;

        /// a curve used to define how distance to the origin should be altered
        [MMLabel("法线偏移曲线")]
        [Tooltip("定义法线轴偏移量的调整曲线")]
        [MMCondition("UseNormalAxisOffsetCurve", true)]
        public AnimationCurve NormalOffsetCurve = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(1, 1f));

        /// the value to which the curve's zero should be remapped to
        [MMLabel("曲线零点重映射")]
        [Tooltip("将曲线起始点(0)映射的具体数值")]
        [MMCondition("UseNormalAxisOffsetCurve", true)]
        public float NormalOffsetCurveRemapZero = 0f;

        /// the value to which the curve's one should be remapped to
        [MMLabel("曲线终点重映射")]
        [Tooltip("将曲线结束点(1)映射的具体数值")]
        [MMCondition("UseNormalAxisOffsetCurve", true)]
        public float NormalOffsetCurveRemapOne = 1f;

        /// whether or not to invert the curve (horizontally)
        [MMLabel("反转曲线")]
        [Tooltip("是否水平翻转曲线形状")]
        [MMCondition("UseNormalAxisOffsetCurve", true)]
        public bool InvertNormalOffsetCurve = false;

        [Header("Rotation")]
        /// the minimum random rotation to apply (in degrees)
        [MMLabel("最小旋转角度")]
        [Tooltip("随机旋转的最小角度值（单位：度）")]
        public Vector3 MinimumRotation = Vector3.zero;

        /// the maximum random rotation to apply (in degrees)
        [MMLabel("最大旋转角度")]
        [Tooltip("随机旋转的最大角度值（单位：度）")]
        public Vector3 MaximumRotation = Vector3.zero;

        [Header("Scale")]
        /// the minimum random scale to apply
        [MMLabel("最小缩放比例")]
        [Tooltip("随机缩放的最小比例值")]
        public Vector3 MinimumScale = Vector3.one;

        /// the maximum random scale to apply
        [MMLabel("最大缩放比例")]
        [Tooltip("随机缩放的最大比例值")]
        public Vector3 MaximumScale = Vector3.one;
    }
    
	/// <summary>
	/// This static class is a spawn helper, useful to randomize position, rotation and scale when you need to
	/// instantiate objects  
	/// </summary>
	public static class MMSpawnAround 
	{
		public static void ApplySpawnAroundProperties(GameObject instantiatedObj, MMSpawnAroundProperties props, Vector3 origin)
		{            
			// we randomize the position
			instantiatedObj.transform.position = SpawnAroundPosition(props, origin);
			// we randomize the rotation
			instantiatedObj.transform.rotation = SpawnAroundRotation(props);
			// we randomize the scale
			instantiatedObj.transform.localScale = SpawnAroundScale(props);
		}

		/// <summary>
		/// Returns the position at which the object should spawn
		/// </summary>
		/// <param name="props"></param>
		/// <param name="origin"></param>
		/// <returns></returns>
		public static Vector3 SpawnAroundPosition(MMSpawnAroundProperties props, Vector3 origin)
		{
			// we get the position of the object based on the defined plane and distance
			Vector3 newPosition;
			if (props.Shape == MMSpawnAroundProperties.MMSpawnAroundShapes.Sphere)
			{
				float distance = Random.Range(props.MinimumSphereRadius, props.MaximumSphereRadius);
				newPosition = Random.insideUnitSphere;
				if (props.ForcePlane)
				{
					newPosition = Vector3.Cross(newPosition, props.NormalToSpawnPlane);	
				}
				
				newPosition.Normalize();
				newPosition *= distance;
			}
			else
			{
				newPosition = PickPositionInsideCube(props);
				if (props.ForcePlane)
				{
					newPosition = Vector3.Cross(newPosition, props.NormalToSpawnPlane); 
				}
			}

			float randomOffset = Random.Range(props.MinimumNormalAxisOffset, props.MaximumNormalAxisOffset);
			// we correct the position based on the NormalOffsetCurve
			if (props.UseNormalAxisOffsetCurve)
			{
				float normalizedOffset = 0f;
				if (randomOffset != 0)
				{
					if (props.InvertNormalOffsetCurve)
					{
						normalizedOffset = MMMaths.Remap(randomOffset, props.MinimumNormalAxisOffset, props.MaximumNormalAxisOffset, 1f, 0f);
					}
					else
					{
						normalizedOffset = MMMaths.Remap(randomOffset, props.MinimumNormalAxisOffset, props.MaximumNormalAxisOffset, 0f, 1f);
					}
				}

				float offset = props.NormalOffsetCurve.Evaluate(normalizedOffset);
				offset = MMMaths.Remap(offset, 0f, 1f, props.NormalOffsetCurveRemapZero, props.NormalOffsetCurveRemapOne);

				newPosition *= offset;
			}
			// we apply the normal offset
			newPosition += props.NormalToSpawnPlane.normalized * randomOffset;

			// relative position
			newPosition += origin;

			return newPosition;
		}

		public static Vector3 PickPositionInsideCube(MMSpawnAroundProperties props)
		{
			int iterationsCount = 0;
			int maxIterationsCount = 1000;
			while (iterationsCount < maxIterationsCount)
			{
				float randomX = Random.Range(0f, props.MaximumCubeBaseSize.x);
				float randomY = Random.Range(0f, props.MaximumCubeBaseSize.y);
				float randomZ = Random.Range(0f, props.MaximumCubeBaseSize.z);
				
				if (randomX < props.MinimumCubeBaseSize.x && randomY < props.MinimumCubeBaseSize.y && randomZ < props.MinimumCubeBaseSize.z)
				{
					iterationsCount++;
					continue;
				}
				else
				{
					randomX = MMMaths.RollADice(2) > 1 ? -randomX : randomX;
					randomY = MMMaths.RollADice(2) > 1 ? -randomY : randomY;
					randomZ = MMMaths.RollADice(2) > 1 ? -randomZ : randomZ;
					return new Vector3(randomX, randomY, randomZ);
				}
			}
			return Vector3.zero;
		}

		/// <summary>
		/// Returns the scale at which the object should spawn
		/// </summary>
		/// <param name="props"></param>
		/// <returns></returns>
		public static Vector3 SpawnAroundScale(MMSpawnAroundProperties props)
		{
			return MMMaths.RandomVector3(props.MinimumScale, props.MaximumScale);
		}

		/// <summary>
		/// Returns the rotation at which the object should spawn
		/// </summary>
		/// <param name="props"></param>
		/// <returns></returns>
		public static Quaternion SpawnAroundRotation(MMSpawnAroundProperties props)
		{
			return Quaternion.Euler(MMMaths.RandomVector3(props.MinimumRotation, props.MaximumRotation));
		}

		/// <summary>
		/// Draws gizmos to show the shape of the spawn area
		/// </summary>
		/// <param name="props"></param>
		/// <param name="origin"></param>
		/// <param name="quantity"></param>
		/// <param name="size"></param>
		public static void DrawGizmos(MMSpawnAroundProperties props, Vector3 origin, int quantity, float size, Color gizmosColor)
		{
			Gizmos.color = gizmosColor;
			for (int i = 0; i < quantity; i++)
			{
				Gizmos.DrawCube(SpawnAroundPosition(props, origin), SpawnAroundScale(props) * size);
			}
		}
	}
}