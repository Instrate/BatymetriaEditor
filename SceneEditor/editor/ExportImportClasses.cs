using Newtonsoft.Json;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace SceneEditor.editor
{
    public class PointValue
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        [JsonConstructor]
        public PointValue(float X, float Y, float Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public PointValue(Vector3 data)
        {
            this.X = data.X;
            this.Y = data.Y;
            this.Z = data.Z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)X, (float)Y, (float)Z);
        }

        public PointValue() { }

        public void WriteStream(JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(X));
            writer.WriteValue(X);
            writer.WritePropertyName(nameof(Y));
            writer.WriteValue(Y);
            writer.WritePropertyName(nameof(Z));
            writer.WriteValue(Z);
            writer.WriteEndObject();
        }
    }

    public class TriangleInfo
    {
        public PointValue[] Vertices { get; set; }

        [JsonConstructor]
        public TriangleInfo(PointValue[] Vertices)
        {
            this.Vertices = Vertices;
        }

        public Vector3[] ToVector3Set()
        {
            Vector3[] result = new Vector3[Vertices.Length];
            for(int i = 0; i < Vertices.Length; i++)
            {
                result[i] = Vertices[i].ToVector3();
            }
            return result;
        }

        public TriangleInfo(Vector3[] Vertices)
        {
            this.Vertices = new PointValue[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                this.Vertices[i] = new PointValue(Vertices[i].X, Vertices[i].Y, Vertices[i].Z);
            }
        }

        public void WriteStream(JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(Vertices));
            writer.WriteStartArray();
            foreach (var v in Vertices)
            {
                v.WriteStream(writer);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }

    public class TriangularedDataSet
    {
        public TriangleInfo[] Triangles { get; set; }

        [JsonConstructor]
        public TriangularedDataSet(TriangleInfo[] Triangles)
        {
            this.Triangles = Triangles;
        }

        public TriangularedDataSet(Vector3[][] Triangles)
        {
            List<TriangleInfo> list = new();
            foreach (var t in Triangles)
            {
                list.Add(new TriangleInfo(t));
            }
            this.Triangles = list.ToArray();
        }

        public void WriteStream(JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(Triangles));
            writer.WriteStartArray();
            foreach (var t in Triangles)
            {
                t.WriteStream(writer);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }

    public class PointsDataSet
    {
        public List<PointValue> Points { get; set; }

        [JsonConstructor]
        public PointsDataSet(List<PointValue> Points)
        {
            this.Points = Points;
        }

        public PointsDataSet(PointValue[] Points)
        {
            this.Points = new (); 
            foreach(var p in Points)
            {
                this.Points.Add(p);
            }
        }

        public PointsDataSet(Vector3[] Points)
        {
            List<PointValue> list = new();
            foreach (var p in Points)
            {
                list.Add(new PointValue(p));
            }
            this.Points = list;
        }

        public Vector3[] ToVector3Set()
        {
            PointValue[] Points = this.Points.ToArray();
            Vector3[] result = new Vector3[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                result[i] = Points[i].ToVector3();
            }
            return result;
        }

        public void WriteStream(JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(Points));
            writer.WriteStartArray();
            foreach (var p in Points)
            {
                p.WriteStream(writer);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }

    public class TileDataSet
    {
        public float[] X { get; set; }
        public float[] Y { get; set; }
        public float[][] Z { get; set; }

        [JsonConstructor]
        public TileDataSet(float[] X, float[] Y, float[][] Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        private void _writeStreamArray(JsonWriter writer, string name, float[] value)
        {
            writer.WritePropertyName(name);
            writer.WriteStartArray();
            foreach(var v in value)
            {
                writer.WriteValue(v);
            }
            writer.WriteEndArray();
        }

        private void _writeStreamArray(JsonWriter writer, string name, float[][] value)
        {
            writer.WritePropertyName(name);
            writer.WriteStartArray();
            foreach (var v in value)
            {
                writer.WriteStartArray();
                foreach (var va in v)
                {
                    writer.WriteValue(va);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndArray();
        }

        public void WriteStreamTileSafe(JsonWriter writer)
        {
            writer.WriteStartObject();
            _writeStreamArray(writer, nameof(X), X);
            _writeStreamArray(writer, nameof(Y), Y);
            _writeStreamArray(writer, nameof(Z), Z);
            writer.WriteEndObject();
        }

        public void WriteStreamPointsDataset(JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(PointsDataSet.Points));
            writer.WriteStartArray();
            for (int i = 0; i < X.Length; i++)
            {
                for(int j = 0; j < Y.Length; j++)
                {
                    var p = new PointValue(X[i], Y[i], Z[i][j]);
                    p.WriteStream(writer);
                }
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}
