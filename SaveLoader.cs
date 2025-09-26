using RayMarch.Objects;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace RayMarch
{
    public static class SaveLoader
    {
        public static void SaveCurrentScene(Scene currentScene)
        {
            var folderBrowserDialog = new FolderBrowserDialog();
            var result = folderBrowserDialog.ShowDialog();

            string folderPath = "";
            if (result == DialogResult.OK)
            {
                folderPath = folderBrowserDialog.SelectedPath;
            }
            else
            {
                MessageBox.Show("Failed to save scene", "Raymarcher");
                return;
            }

            string filePath = Path.Combine(folderPath, "new_scene.csv");
            int counter = 1;
            while (File.Exists(filePath))
            {
                filePath = Path.Combine(folderPath, $"new_scene{counter}.csv");
                counter++;
            }

            var writer = new StreamWriter(filePath);

            string[] headers =
            {   
                "Type",
                "PositionX", "PositionY", "PositionZ",
                "RotationX", "RotationY", "RotationZ",
                "LinearVelocityX", "LinearVelocityY", "LinearVelocityZ",
                "AngularVelocityX", "AngularVelocityY", "AngularVelocityZ",
                "ColorR", "ColorG", "ColorB", "ColorA",
                "Reflectivity",
                "TypeProp1", "TypeProp2", "TypeProp3"
            };

            writer.WriteLine(string.Join(",", headers));

            foreach (IObject obj in currentScene.Objects)
            {
                var row = new List<string>()
                {
                        obj.GetType().Name,
                        obj.Position.X.ToString(),
                        obj.Position.Y.ToString(),
                        obj.Position.Z.ToString(),
                        obj.Rotation.X.ToString(),
                        obj.Rotation.Y.ToString(),
                        obj.Rotation.Z.ToString(),
                        obj.LinearVelocity.X.ToString(),
                        obj.LinearVelocity.Y.ToString(),
                        obj.LinearVelocity.Z.ToString(),
                        obj.AngularVelocity.X.ToString(),
                        obj.AngularVelocity.Y.ToString(),
                        obj.AngularVelocity.Z.ToString(),
                        obj.Color.X.ToString(),
                        obj.Color.Y.ToString(),
                        obj.Color.Z.ToString(),
                        obj.Color.W.ToString(),
                        obj.Reflectivity.ToString(),
                };
                if (obj is Sphere sphere)
                {
                    row.Add(sphere.Radius.ToString()); // TypeProp1
                    row.Add(""); // empty TypeProp2
                    row.Add(""); // empty TypeProp3
                }
                else if (obj is Light light)
                {
                    row.Add(light.Radius.ToString()); // TypeProp1
                    row.Add(""); // empty TypeProp2
                    row.Add(""); // empty TypeProp3
                }
                else if (obj is Box box)
                {
                    row.Add(box.Size.X.ToString()); // TypeProp1
                    row.Add(box.Size.Y.ToString()); // TypeProp2
                    row.Add(box.Size.Z.ToString()); // TypeProp3
                }
                else if (obj is Torus torus)
                {
                    row.Add(torus.RadiusToroidal.ToString()); // TypeProp1
                    row.Add(torus.RadiusPoloidal.ToString()); // TypeProp2
                    row.Add(""); // empty TypeProp3
                }
                else if (obj is Capsule capsule)
                {
                    row.Add(capsule.Radius.ToString()); // TypeProp1
                    row.Add(capsule.Height.ToString()); // TypeProp2
                    row.Add(""); // empty TypeProp3
                }

                writer.WriteLine(string.Join(",", row));
            }

            writer.Close();
        }
        public static Scene LoadSceneFromFies()
        {
            var scene = new Scene();

            var openFileDialog = new OpenFileDialog();
            var result = openFileDialog.ShowDialog();

            string filePath = "";
            if (result == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
            }
            else
            {
                MessageBox.Show("Failed to load scene", "Raymarcher");
                return null;
            }

            var sceneData = new List<string[]>();

            var reader = new StreamReader(filePath);

            string headerLine = reader.ReadLine();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                string type = values[0];

                // Common properties
                var position = new Vector3(
                    float.Parse(values[1]),
                    float.Parse(values[2]),
                    float.Parse(values[3])
                );
                var rotation = new Vector3(
                    float.Parse(values[4]),
                    float.Parse(values[5]),
                    float.Parse(values[6])
                );
                var linearVelocity = new Vector3(
                    float.Parse(values[7]),
                    float.Parse(values[8]),
                    float.Parse(values[9])
                );
                var angularVelocity = new Vector3(
                    float.Parse(values[10]),
                    float.Parse(values[11]),
                    float.Parse(values[12])
                );
                var color = new Vector4(
                    float.Parse(values[13]),
                    float.Parse(values[14]),
                    float.Parse(values[15]),
                    float.Parse(values[16])
                );
                float reflectivity = float.Parse(values[17]);

                IObject obj = null;

                switch (type)
                {
                    case "Sphere":
                        obj = new Sphere(position, rotation, color, reflectivity, float.Parse(values[18])); // radius
                        break;

                    case "Light":
                        obj = new Light(position, rotation, new Vector3(color.X, color.Y, color.Z), float.Parse(values[18])); // radius
                        break;

                    case "Box":
                        obj = new Box(position, rotation, color, reflectivity, new Vector3(
                            float.Parse(values[18]), // size x
                            float.Parse(values[19]), // size y
                            float.Parse(values[20])  // size z
                        ));
                        break;

                    case "Torus":
                        obj = new Torus(position, rotation, color, reflectivity,
                            float.Parse(values[18]), // radius toroidal
                            float.Parse(values[19])  // radius poloidal
                        );
                        break;

                    case "Capsule":
                        obj = new Capsule(position, rotation, color, reflectivity,
                            float.Parse(values[18]), // radius
                            float.Parse(values[19])  // height
                        );
                        break;
                }

                if (obj != null) scene.AddObject(obj);
            }

            return scene;

        }
    }
}
