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
		CommandBuffer cmd = CommandBufferPool.Get("Camera Clearer");
		try {
			cmd.SetViewport(new Rect(0, 0, Screen.width, Screen.height));
			cmd.ClearRenderTarget(false, true, cam.backgroundColor.linear);

			ctx.ExecuteCommandBuffer(cmd);
		}
		finally {
			CommandBufferPool.Release(cmd);
		}
	}
}