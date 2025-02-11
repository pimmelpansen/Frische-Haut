using Newtonsoft.Json;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Valve.VR;
using Device = SharpDX.Direct3D11.Device;

public class PlayspaceFloor {
	[StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
	private struct PlayAreaRect {
		[FieldOffset(0 * 16)] public Vector3 corner0;
		[FieldOffset(1 * 16)] public Vector3 corner1;
		[FieldOffset(2 * 16)] public Vector3 corner2;
		[FieldOffset(3 * 16)] public Vector3 corner3;
	}

	private readonly VertexShader vertexShader;
	private readonly PixelShader pixelShader;
	public bool IsVisible { get; set; } = true;
	private ConstantBufferManager<PlayAreaRect> playAreaRectBuffer;

	public PlayspaceFloor(Device device, ShaderCache shaderCache) {
		vertexShader = shaderCache.GetVertexShader<Backdrop>("backdrop/PlayspaceFloor");
		pixelShader = shaderCache.GetPixelShader<Backdrop>("backdrop/PlayspaceFloor");
		playAreaRectBuffer = new ConstantBufferManager<PlayAreaRect>(device);
	}

	public void Dispose() {
		playAreaRectBuffer.Dispose();
	}
	
	public void Update(DeviceContext context) {
		PlayAreaRect value;

		HmdQuad_t rect = default(HmdQuad_t);
		if (OpenVR.Chaperone.GetPlayAreaRect(ref rect)) {
			value = new PlayAreaRect {
				corner0 = rect.vCorners0.Convert(),
				corner1 = rect.vCorners1.Convert(),
				corner2 = rect.vCorners3.Convert(),
				corner3 = rect.vCorners2.Convert()
			};
		} else {
			float halfSize = 1.5f;
			value = new PlayAreaRect {
				corner0 = new Vector3(-halfSize, 0, +halfSize),
				corner1 = new Vector3(+halfSize, 0, +halfSize),
				corner2 = new Vector3(-halfSize, 0, -halfSize),
				corner3 = new Vector3(+halfSize, 0, -halfSize),
			};
		}
		
		playAreaRectBuffer.Update(context, value);
	}

	public void Render(DeviceContext context, bool depthOnly) {
		if (!IsVisible) {
			return;
		}

		context.VertexShader.Set(vertexShader);
		context.VertexShader.SetConstantBuffer(1, playAreaRectBuffer.Buffer);
		context.PixelShader.Set(depthOnly ? null : pixelShader);
		context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
		context.Draw(4, 0);
	}

	public class Recipe {
		[JsonProperty("visible", DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue(true)]
		public bool isVisible;

		public void Merge(PlayspaceFloor floor) {
			floor.IsVisible = isVisible;
		}
	}

	public Recipe Recipize() {
		return new Recipe {
			isVisible = IsVisible
		};
	}
}
