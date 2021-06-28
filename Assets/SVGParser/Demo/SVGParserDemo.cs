using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using seyself;

public class SVGParserDemo : MonoBehaviour
{
    void Start()
    {
        SVGParser parser = new SVGParser();
        List<SVGPath> svgPath = parser.Parse("Data/sample.svg");
        // List<SVGPath> svgPath = parser.ParseText( FileIO.ReadText("Data/sample.svg") );
        SVGViewer viewer = gameObject.AddComponent<SVGViewer>();
        viewer.Draw(svgPath);
    }
}
