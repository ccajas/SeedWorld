using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SeedWorld
{
    class SceneCuller
    {
        class MeshDistanceSort : IComparer<MeshInstance>
        {
            public int Compare(MeshInstance instance1, MeshInstance instance2)
            {
                return instance1.distance.CompareTo(instance2.distance);
            }
        }

        /// <summary>
        /// Culls all possible objects in a scene 
        /// </summary>

        public class SceneCuller
        {
            /// Cached list of instances from last model culling.
            private List<MeshInstance> visibleInstances = new List<MeshInstance>();

            /// Minimum distance to limit full mesh rendering.
            public float maxLODdistance = 25000f;

            /// Number of meshes culled so far
            public int culledMeshes { private set; get; }

            /// <summary>
            /// Cull an InstancedModel and its mesh groups.
            /// </summary>

            public void CullModelInstances(Camera camera, Model instancedModel)
            {
                int meshIndex = 0;
                foreach (MeshInstanceGroup instanceGroup in instancedModel.MeshInstanceGroups.Values)
                {
                    // Pre-cull mesh parts
                    instanceGroup.totalVisible = 0;

                    foreach (MeshInstance meshInstance in instanceGroup.instances)
                    {
                        // Add mesh and instances to visible list if they're contained in the frustum
                        if (camera.frustum.Contains(meshInstance.boundingSphere) != ContainmentType.Disjoint)
                            instanceGroup.visibleInstances[instanceGroup.totalVisible++] = meshInstance;
                    }

                    int fullMeshInstances = 0;

                    // Out of the visible instances, sort those by distance
                    for (int i = 0; i < instanceGroup.totalVisible; i++)
                    {
                        MeshInstance meshInstance = instanceGroup.visibleInstances[i];
                        meshInstance.distance = Vector3.Distance(camera.position, meshInstance.position);

                        // Use a second loop-through to separate the full meshes from the imposters.
                        // Meshes closer than the limit distance will be moved to the front of the list,
                        // and those beyond will be put into a separate bucket for imposter rendering.

                        if (meshInstance.distance < maxLODdistance)
                            instanceGroup.visibleInstances[fullMeshInstances++] = meshInstance;
                    }
                    // Update the new, filtered amount of full meshes to draw
                    instanceGroup.totalVisible = fullMeshInstances;
                    //instanceGroup.instances.Sort((a, b) => a.distance.CompareTo(b.distance));

                    meshIndex++;
                }
                // Finished culling this model
            }

            /// <summary>
            /// Check all meshes in a scene that are outside the view frustum.
            /// </summary>

            public void CullModelMeshes(Scene scene, Camera camera)
            {
                culledMeshes = 0;
                CullFromList(camera, scene.sceneModels);
            }

            /// <summary>
            /// Wrapper to cull meshes from a specified list.
            /// </summary>

            public void CullFromList(Camera camera, Dictionary<String, Model> modelList)
            {
                visibleInstances.Clear();

                foreach (Model instancedModel in modelList.Values)
                    CullModelInstances(camera, instancedModel);

                // Finished culling all models
                int total = visibleInstances.Count;
            }

            /// <summary>
            /// Remove any lights outside of the viewable frustum.
            /// </summary>

            public void CullLights(Scene scene, Camera camera)
            {
                Vector3 lightPosition = Vector3.Zero;
                Vector3 radiusVector = Vector3.Zero;

                // Refresh the list of visible point lights
                scene.visiblePointLights.Clear();
                BoundingSphere bounds = new BoundingSphere();

                // Pre-cull point lights
                foreach (PointLight light in scene.pointLights)
                {
                    lightPosition.X = light.instance.transform.M41;
                    lightPosition.Y = light.instance.transform.M42;
                    lightPosition.Z = light.instance.transform.M43;

                    radiusVector.X = light.instance.transform.M11;
                    radiusVector.Y = light.instance.transform.M12;
                    radiusVector.Z = light.instance.transform.M13;

                    float radius = radiusVector.Length();

                    // Create bounding sphere to check which lights are in view

                    bounds.Center = lightPosition;
                    bounds.Radius = radius;

                    if (camera.frustum.Contains(bounds) != ContainmentType.Disjoint)
                    {
                        scene.visiblePointLights.Add(light);
                    }
                }
                // Finished culling lights
            }
        }
    }
}
