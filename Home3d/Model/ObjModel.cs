﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using Tao.OpenGl;

namespace Home3d.Model
{
    public class ObjModel
    {
        public ObjModel()
        {
            Vertices = new List<ObjVertex>();
            ComputedNormals = new List<ObjNormal>();
            Normals = new List<ObjNormal>();
            Textures = new List<ObjTexture>();
            Objects = new Dictionary<string, ObjObject>();
            Materials = new Dictionary<string, ObjMaterial>();
            MaximumVertex = new ObjVertex();
            MinimumVertex = new ObjVertex();
        }

        public List<ObjVertex> Vertices { get; set; }
        public List<ObjNormal> ComputedNormals { get; set; } 
        public List<ObjNormal> Normals { get; set; }
        public List<ObjTexture> Textures { get; set; }
        public Dictionary<string, ObjObject> Objects { get; set; }
        public Dictionary<string, ObjMaterial> Materials { get; set; }

        public ObjVertex MinimumVertex { get; private set; }
        public ObjVertex MaximumVertex { get; private set; }

        public void CalculateMaxAndMinVertex()
        {
            MinimumVertex.X = Double.MaxValue;
            MinimumVertex.Y = Double.MaxValue;
            MinimumVertex.Z = Double.MaxValue;

            MaximumVertex.X = Double.MinValue;
            MaximumVertex.Y = Double.MinValue;
            MaximumVertex.Z = Double.MinValue;

            foreach (var objVertex in Vertices)
            {
                if (MinimumVertex.X > objVertex.X)
                {
                    MinimumVertex.X = objVertex.X;
                }
                if (MinimumVertex.Y > objVertex.Y)
                {
                    MinimumVertex.Y = objVertex.Y;
                }
                if (MinimumVertex.Z > objVertex.Z)
                {
                    MinimumVertex.Z = objVertex.Z;
                }

                if (MaximumVertex.X < objVertex.X)
                {
                    MaximumVertex.X = objVertex.X;
                }
                if (MaximumVertex.Y < objVertex.Y)
                {
                    MaximumVertex.Y = objVertex.Y;
                }
                if (MaximumVertex.Z < objVertex.Z)
                {
                    MaximumVertex.Z = objVertex.Z;
                }
            }
        }

        public bool Load(string objModelPath, string objMaterialPath)
        {
            if (!File.Exists(objMaterialPath) || !File.Exists(objMaterialPath))
            {
                return false;
            }

            var objModelStreamReader = new StreamReader(objModelPath);
            var objMaterialStreamReader = new StreamReader(objMaterialPath);

            string line;
            var lastObjectMaterialRead = string.Empty;
            var lastObjectNameRead = string.Empty;
            var lastMaterialNameRead = string.Empty;

            while ((line = objMaterialStreamReader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var splittedLine = line.Split(' ');
                var splittedLineSize = splittedLine.Length;

                if (splittedLineSize == 0)
                {
                    continue;
                }

                if (splittedLine[0] == "#")
                {
                    continue;
                }

                if (splittedLine[0] == "newmtl" && splittedLineSize >= 2)
                {
                    var materialName = string.Empty;

                    for (int index = 1; index < splittedLineSize; index++)
                    {
                        materialName += splittedLine[index];
                    }

                    lastMaterialNameRead = materialName;
                    Materials[lastMaterialNameRead] = new ObjMaterial(lastMaterialNameRead);
                    continue;
                }

                if (splittedLine[0] == "Ka" && splittedLineSize >= 4)
                {
                    if (!Materials.ContainsKey(lastMaterialNameRead))
                    {
                        return false;
                    }
                    var material = Materials[lastMaterialNameRead];
                    material.AmbientColor.Red = double.Parse(splittedLine[1],CultureInfo.InvariantCulture);
                    material.AmbientColor.Green = double.Parse(splittedLine[2], CultureInfo.InvariantCulture);
                    material.AmbientColor.Blue = double.Parse(splittedLine[3], CultureInfo.InvariantCulture);
                    if (splittedLineSize >= 5)
                    {
                        material.AmbientColor.Alpha = double.Parse(splittedLine[4], CultureInfo.InvariantCulture);
                    }
                    continue;
                }

                if (splittedLine[0] == "Kd" && splittedLineSize >= 4)
                {
                    if (!Materials.ContainsKey(lastMaterialNameRead))
                    {
                        return false;
                    }
                    var material = Materials[lastMaterialNameRead];
                    material.DiffuseColor.Red = double.Parse(splittedLine[1], CultureInfo.InvariantCulture);
                    material.DiffuseColor.Green = double.Parse(splittedLine[2], CultureInfo.InvariantCulture);
                    material.DiffuseColor.Blue = double.Parse(splittedLine[3], CultureInfo.InvariantCulture);
                    if (splittedLineSize >= 5)
                    {
                        material.DiffuseColor.Alpha = double.Parse(splittedLine[4], CultureInfo.InvariantCulture);
                    }
                    continue;
                }

                if (splittedLine[0] == "Ks" && splittedLineSize >= 4)
                {
                    if (!Materials.ContainsKey(lastMaterialNameRead))
                    {
                        return false;
                    }
                    var material = Materials[lastMaterialNameRead];
                    material.SpecularColor.Red = double.Parse(splittedLine[1], CultureInfo.InvariantCulture);
                    material.SpecularColor.Green = double.Parse(splittedLine[2], CultureInfo.InvariantCulture);
                    material.SpecularColor.Blue = double.Parse(splittedLine[3], CultureInfo.InvariantCulture);
                    if (splittedLineSize >= 5)
                    {
                        material.SpecularColor.Alpha = double.Parse(splittedLine[4], CultureInfo.InvariantCulture);
                    }
                    continue;
                }

                if (splittedLine[0] == "Ns" && splittedLineSize >= 2)
                {
                    if (!Materials.ContainsKey(lastMaterialNameRead))
                    {
                        return false;
                    }
                    var material = Materials[lastMaterialNameRead];
                    material.SpecularExponent = double.Parse(splittedLine[1], CultureInfo.InvariantCulture);
                    continue;
                }

                if ((splittedLine[0] == "d" || splittedLine[0] == "Tr")
                        && splittedLineSize >= 2)
                {
                    if (!Materials.ContainsKey(lastMaterialNameRead))
                    {
                        return false;
                    }
                    var material = Materials[lastMaterialNameRead];
                    material.Transparency = double.Parse(splittedLine[1], CultureInfo.InvariantCulture);
                    continue;
                }

                if (splittedLine[0] == "illum" && splittedLineSize >= 2)
                {
                    if (!Materials.ContainsKey(lastMaterialNameRead))
                    {
                        return false;
                    }
                    var material = Materials[lastMaterialNameRead];
                    material.Illumination = int.Parse(splittedLine[1], CultureInfo.InvariantCulture);
                    continue;
                }

                if (splittedLine[0] == "Ni")
                {
                    continue;
                }

                return false;
            }

            while ((line = objModelStreamReader.ReadLine()) != null)
            {
                var splittedLine = line.Split(' ');
                var splittedLineSize = splittedLine.Length;

                if (splittedLineSize == 0)
                {
                    continue;
                }

                if (splittedLine[0] == "#")
                {
                    continue;
                }

                if (splittedLine[0] == "v" && splittedLineSize >= 4)
                {
                    var vertex = new ObjVertex
                    {
                        X = double.Parse(splittedLine[1], CultureInfo.InvariantCulture),
                        Y = double.Parse(splittedLine[2], CultureInfo.InvariantCulture),
                        Z = double.Parse(splittedLine[3], CultureInfo.InvariantCulture)
                    };
                    Vertices.Add(vertex);
                    continue;
                }

                if (splittedLine[0] == "vt" && splittedLineSize >= 3)
                {
                    var texture = new ObjTexture
                    {
                        X = double.Parse(splittedLine[1], CultureInfo.InvariantCulture),
                        Y = double.Parse(splittedLine[2], CultureInfo.InvariantCulture)
                    };
                    Textures.Add(texture);
                    continue;
                }

                if (splittedLine[0] == "vn" && splittedLineSize >= 4)
                {
                    var normal = new ObjNormal
                    {
                        X = double.Parse(splittedLine[1], CultureInfo.InvariantCulture),
                        Y = double.Parse(splittedLine[2], CultureInfo.InvariantCulture),
                        Z = double.Parse(splittedLine[3], CultureInfo.InvariantCulture)
                    };
                    Normals.Add(normal);
                    continue;
                }

                if (splittedLine[0] == "f")
                {
                    if (Objects.Count == 0)
                    {
                        Objects["Untitled"] = new ObjObject("Untitled");
                        lastObjectNameRead = "Untitled";
                    }

                    var newFace = new ObjFace(lastObjectMaterialRead);

                    for (var index = 1; index < splittedLineSize; index++)
                    {
                        var faceItem = new ObjFaceItem();
                        var tokens = splittedLine[index].Split('/');
                        if (tokens.Length == 0)
                        {
                            return false;
                        }
                        if (tokens.Length >= 1)
                        {
                            if (string.IsNullOrWhiteSpace(tokens[0]))
                            {
                                return false;
                            }
                            faceItem.VertexIndex = int.Parse(tokens[0], CultureInfo.InvariantCulture) - 1;
                        }
                        if (tokens.Length >= 2 && !string.IsNullOrWhiteSpace(tokens[1]))
                        {
                            faceItem.TextureIndex = int.Parse(tokens[1], CultureInfo.InvariantCulture) - 1;
                        }
                        if (tokens.Length >= 3 && !string.IsNullOrWhiteSpace(tokens[2]))
                        {
                            faceItem.NormalIndex = int.Parse(tokens[2], CultureInfo.InvariantCulture) - 1;
                        }
                        newFace.FaceItems.Add(faceItem);
                    }

                    Objects[lastObjectNameRead].Faces.Add(newFace);
                    continue;
                }

                if (splittedLine[0] == "o" && splittedLineSize >= 2)
                {
                    var objectName = string.Empty;
                    for (var index = 1; index < splittedLineSize; index++)
                    {
                        objectName += splittedLine[index];
                    }
                    Objects[objectName] = new ObjObject(objectName);
                    lastObjectNameRead = objectName;
                    continue;
                }

                if (splittedLine[0] == "usemtl" && splittedLineSize >= 2)
                {
                    var materialName = string.Empty;
                    for (var index = 1; index < splittedLineSize; index++)
                    {
                        materialName += splittedLine[index];
                    }
                    lastObjectMaterialRead = materialName;
                    continue;
                }

                if (splittedLine[0] == "g")
                {
                    // TODO : maybe handle groups?
                    continue;
                }

                if (splittedLine[0] == "s")
                {
                    // TODO : maybe handle smoothness (not necesary)
                    continue;
                }

                if (splittedLine[0] == "mtllib")
                {
                    // No need to handle this, we can give the paths manually to the material
                    // files.
                    continue;
                }
                return false;
            }

            // Calulcate min vetexes and max vertexes
            CalculateMaxAndMinVertex();

            // Build the objects into GPU
            foreach (var modelObject in Objects.Values)
            {
                var lastFaceMaterial = string.Empty;
                Gl.glNewList(modelObject.ListId, Gl.GL_COMPILE);
                foreach (var face in modelObject.Faces)
                {
                    if ((lastFaceMaterial == string.Empty) || (lastFaceMaterial != string.Empty && face.MaterialName != lastFaceMaterial && Materials.ContainsKey(face.MaterialName)))
                    {
                        var material = Materials[face.MaterialName];
                        Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT, material.AmbientColor.ToFloatArray());
                        Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_DIFFUSE, material.DiffuseColor.ToFloatArray());
                        Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_EMISSION, material.DiffuseColor.ToFloatArray());
                        Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_SPECULAR, material.SpecularColor.ToFloatArray());
                    }
                    Gl.glShadeModel(Gl.GL_SMOOTH);
                    Gl.glBegin(Gl.GL_POLYGON);
                    foreach (var faceItem in face.FaceItems)
                    {
                        if (faceItem.NormalIndex != -1)
                        {
                            var normal = Normals[faceItem.NormalIndex];
                            Gl.glNormal3d(normal.X, normal.Y, normal.Z);
                        }
                        var vertex = Vertices[faceItem.VertexIndex];
                        Gl.glVertex3d(vertex.X, vertex.Y, vertex.Z);
                    }
                    Gl.glEnd();
                    lastFaceMaterial = face.MaterialName;
                }
                Gl.glEndList();
            }

            return true;
        }
    }
}