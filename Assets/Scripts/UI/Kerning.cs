using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*

Produces an simple tracking/letter-spacing effect on UI Text components.

Set the spacing parameter to adjust letter spacing.
  Negative values cuddle the text up tighter than normal. Go too far and it'll look odd.
  Positive values spread the text out more than normal. This will NOT respect the text area you've defined.
  Zero spacing will present the font with no changes.

Relies on counting off characters in your Text compoennt's text property and
matching those against the quads passed in via the verts array. This is really
rather primative, but I can't see any better way at the moment. It means that
all sorts of things can break the effect...

This component should be placed higher in component list than any other vertex
modifiers that alter the total number of verticies. Eg, place this above Shadow
or Outline effects. If you don't, the outline/shadow won't match the position
of the letters properly. If you place the outline/shadow effect second however,
it will just work on the altered vertices from this component, and function
as expected.

This component works best if you don't allow text to automatically wrap. It also
blows up outside of the given text area. Basically, it's a cheap and dirty effect,
not a clever text layout engine. It can't affect how Unity chooses to break up
your lines. If you manually use line breaks however, it should detect those and
function more or less as you'd expect.

The spacing parameter is measured in pixels multiplied by the font size. This was
chosen such that when you adjust the font size, it does not change the visual spacing
that you've dialed in. There's also a scale factor of 1/100 in this number to
bring it into a comfortable adjustable range. There's no limit on this parameter,
but obviously some values will look quite strange.

This component doesn't really work with Rich Text. You don't need to remember to
turn off Rich Text via the checkbox, but because it can't see what makes a
printable character and what doesn't, it will typically miscount characters when you
use HTML-like tags in your text. Try it out, you'll see what I mean. It doesn't
break down entirely, but it doesn't really do what you'd want either.

*/

namespace UnityEngine.UI
{
	[AddComponentMenu("UI/Effects/Kerning", 14)]
	public class Kerning : BaseMeshEffect
	{
		[SerializeField]
		private float m_spacing = 0f;
		
		protected Kerning() { }
		
		#if UNITY_EDITOR
		protected override void OnValidate()
		{
			spacing = m_spacing;
			base.OnValidate();
		}
		#endif
		
		public float spacing
		{
			get { return m_spacing; }
			set
			{
				if (m_spacing == value) return;
				m_spacing = value;
				if (graphic != null) graphic.SetVerticesDirty();
			}
		}
		
		public override void ModifyMesh(VertexHelper vh)
		{
			if (! IsActive()) return;
			
			Text text = GetComponent<Text>();
			if (text == null)
			{
				Debug.LogWarning("Kerning: Missing Text component");
				return;
			}
			
			string[] lines = text.text.Split('\n');
			Vector3  pos;
			float    letterOffset    = spacing * (float)text.fontSize / 100f;
			float    alignmentFactor = 0;
			int      glyphIdx        = 0;
			
			switch (text.alignment)
			{
			case TextAnchor.LowerLeft:
			case TextAnchor.MiddleLeft:
			case TextAnchor.UpperLeft:
				alignmentFactor = 0f;
				break;
				
			case TextAnchor.LowerCenter:
			case TextAnchor.MiddleCenter:
			case TextAnchor.UpperCenter:
				alignmentFactor = 0.5f;
				break;
				
			case TextAnchor.LowerRight:
			case TextAnchor.MiddleRight:
			case TextAnchor.UpperRight:
				alignmentFactor = 1f;
				break;
			}

            List<UIVertex> verts = new List<UIVertex>();
            vh.GetUIVertexStream(verts);
			
			for (int lineIdx=0; lineIdx < lines.Length; lineIdx++)
			{
				string line = lines[lineIdx];
				float lineOffset = (line.Length -1) * letterOffset * alignmentFactor;
				
				for (int charIdx = 0; charIdx < line.Length; charIdx++)
				{
                    pos = Vector3.right * (letterOffset * charIdx - lineOffset);

                    for (int id = 0; id < 4; id++)
                    {
					    int idx = glyphIdx * 4 + id;
                        if (idx > verts.Count - 1) return; // Check for truncated text (doesn't generate verts for all characters)
                        UIVertex vertex = new UIVertex();
                        vh.PopulateUIVertex(ref vertex, idx);
					    vertex.position += pos;
                        vh.SetUIVertex(vertex, idx);
                    }

                    glyphIdx++;
                }
				
				// Offset for carriage return character that still generates verts
				glyphIdx++;
			}
		}
	}
}
