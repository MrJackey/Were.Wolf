using UnityEngine;
using UnityEngine.Rendering;

public class CameraClearer : MonoBehaviour {
	private void OnEnable() {
		RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
	}

	private void OnDisable() {
		RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
	}

	private void OnBeginCameraRendering(ScriptableRenderContext ctx, Camera cam) {
		CommandBuffer cmd = CommandBufferPool.Get();
		try {
			cmd.SetViewport(new Rect(0, 0, Screen.width, Screen.height));
			// Unity is set up to automatically convert colors from linear space to sRGB. Because of this, we need to
			// make sure we give it a color in linear space.
			cmd.ClearRenderTarget(false, true, cam.backgroundColor.linear);

			ctx.ExecuteCommandBuffer(cmd);
		}
		finally {
			CommandBufferPool.Release(cmd);
		}
	}
}