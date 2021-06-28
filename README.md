# SVGParser for Unity
SVG File Parser

## Parse SVG

```
void Start()
{
    SVGParser parser = new SVGParser();
    List<SVGPath> svgPath = parser.Parse("Data/sample.svg");
    SVGViewer viewer = gameObject.AddComponent<SVGViewer>();
    viewer.Draw(svgPath);
}
```
