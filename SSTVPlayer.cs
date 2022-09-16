using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;

[ExecuteInEditMode]
[CustomEditor(typeof(SSTVPlayer))]
 public class CustomButton : Editor
 {
	 public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		SSTVPlayer myScript = (SSTVPlayer)target;
        if (GUILayout.Button("Play"))
            myScript.GoSstv();
		if (GUILayout.Button("Stop"))
			myScript.StopSSTV();
    }
 }

#endif

public class SSTVPlayer : MonoBehaviour {

	CharacterController cc;

	[SerializeField] Camera playerCam;
	[SerializeField] AudioSource playerAudio;
	
	public void GoSstv() {
		playerAudio.Stop();
		playerAudio.clip = encoder.GetAudioClip(currentScanMode, playerCam);
		playerAudio.Play();
	}
	public void StopSSTV() {
		playerAudio.Stop();
	}

	SSTVEncoder encoder = new SSTVEncoder();
	public SSTVEncoder.SlowScanMode currentScanMode = SSTVEncoder.SlowScanMode.Robot36;
}

//https://github.com/xdsopl/robot36/blob/master/README
public class SSTVEncoder {
	float sampleRate = 8000;
	Vector2 res;
	
	public enum SlowScanMode { BW8, BW12, Martin1, Martin2, Robot36, Robot72, Robot24, Scottie1, Scottie2, ScottieDX };
	public SlowScanMode scanMode;
	
	float[] clipsamples;
	float sineT = 0;
	float timer = 0;
	float step = 0;
	int i = 0;
	
	public AudioClip GetAudioClip (SlowScanMode mode, Camera cam) {
		sineT = 0; timer = 0; i = 0;
		step = 1000f / sampleRate;
		
		scanMode = mode;
		
		switch(scanMode) {
		case SlowScanMode.BW8:
			res = new Vector2(160, 120);
			break;
		case SlowScanMode.BW12:
			res = new Vector2(160, 120);
			break;
		case SlowScanMode.Robot24:
			res = new Vector2(160, 120);
			break;
		case SlowScanMode.Robot36:
			res = new Vector2(320, 240);
			break;
		case SlowScanMode.Robot72:
			res = new Vector2(320, 240);
			break;
		case SlowScanMode.Martin1:
			res = new Vector2(320, 256);
			break;
		case SlowScanMode.Martin2:
			res = new Vector2(320, 256);
			break;
		case SlowScanMode.Scottie1:
			res = new Vector2(320, 256);
			break;
		case SlowScanMode.Scottie2:
			res = new Vector2(320, 256);
			break;
		case SlowScanMode.ScottieDX:
			res = new Vector2(320, 256);
			break;
		}
		
		Texture2D tex2d = ReadyTexture(cam);
		
		AudioClip clip = AudioClip.Create("", tex2d.width * tex2d.height * (int)sampleRate / 100, 1, (int)sampleRate, false);
		
		clipsamples = new float[clip.samples * clip.channels];
		clip.GetData(clipsamples, 0);
		
		if(scanMode == SlowScanMode.BW8 || scanMode == SlowScanMode.BW12) {
			for(int y = tex2d.height; y >= 0; y--) {
				float scanPulse = 6f;
				float scanPorch = 2f;
				
				float scanTime = 0;
				if(scanMode == SlowScanMode.BW8) scanTime = 8000f / tex2d.height - scanPulse - scanPorch + 0.229f;
				else scanTime = 12000f / tex2d.height - scanPulse - scanPorch;
				
				// Sync pulse
				timer += scanPulse;
				while(timer > 0) {
					AddSample(GetSineValue(1200f));
				}
				// Sync porch
				timer += scanPorch;
				while(timer > 0) {
					AddSample(GetSineValue(1500f));
				}
				//Scan
				timer += scanTime;
				while(timer > 0) {
					int x = (int)((scanTime - timer) / scanTime * tex2d.width);
					float gray = tex2d.GetPixel(x, y).grayscale;
					
					AddSample(GetSineValue(1500f + gray * 800));
				}
				sineT = Mathf.Repeat(sineT, Mathf.PI * 2);
			}
		}
		
		if(scanMode == SlowScanMode.Martin1 || scanMode == SlowScanMode.Martin2) {
			float scanTime = 0;
			if(scanMode == SlowScanMode.Martin1) scanTime = 146.432f;
			else scanTime = 73.216f;
			
			for(int y = tex2d.height; y >= 0; y--) {
				// Sync pulse
				timer += 4.862f;
				while(timer > 0) {
					AddSample(GetSineValue(1200f));
				}
				// Sync porch
				timer += .572f;
				while(timer > 0) {
					AddSample(GetSineValue(1500f));
				}
				//Green scan
				timer += scanTime;
				while(timer > 0) {
					int x = (int)((scanTime - timer) / scanTime * tex2d.width);
					float green = tex2d.GetPixel(x, y).g;
					AddSample(GetSineValue(1500f + green * 800));
				}
				//Separator pulse
				timer += .572f;
				while(timer > 0) {
					AddSample(GetSineValue(1500f));
				}
				//Blue scan
				timer += scanTime;
				while(timer > 0) {
					int x = (int)((scanTime - timer) / scanTime * tex2d.width);
					float blue = tex2d.GetPixel(x, y).b;
					AddSample(GetSineValue(1500f + blue * 800));
				}
				//Separator pulse
				timer += .572f;
				while(timer > 0) {
					AddSample(GetSineValue(1500f));
				}
				//Red scan
				timer += scanTime;
				while(timer > 0) {
					int x = (int)((scanTime - timer) / scanTime * tex2d.width);
					float red = tex2d.GetPixel(x, y).r;
					AddSample(GetSineValue(1500f + red * 800));
				}
				//Separator pulse
				timer += .572f;
				while(timer > 0) {
					AddSample(GetSineValue(1500f));
				}
				sineT = Mathf.Repeat(sineT, Mathf.PI * 2);
			}
		}
		
		if(scanMode == SlowScanMode.Robot36) {
			for(int y = tex2d.height; y >= 0; ) {
				for(int n = 0; n < 2; n++) {
					// Sync pulse
					timer += 9f;
					while(timer > 0) {
						AddSample(GetSineValue(1200f));
					}
					// Sync porch
					timer += 3f;
					while(timer > 0) {
						AddSample(GetSineValue(1500f));
					}
					//Y scan
					timer += 88f;
					while(timer > 0) {
						int x = (int)((88f - timer) / 88f * tex2d.width);
						Color pixel = tex2d.GetPixel(x, y);
						float yValue = GetYValue(pixel);
						
						AddSample(GetSineValue(1500f + yValue * 3.1372549f));
					}
					//'Even\Odd' separator pulse
					timer += 4.5f;
					float evenodd = (n == 0)? 1500f: 2300f;
					while(timer > 0) {
						AddSample(GetSineValue(evenodd));
					}
					//Porch
					timer += 1.5f;
					while(timer > 0) {
						AddSample(GetSineValue(1900f));
					}
					// R-Y or B-Y scan
					timer += 44f;
					while(timer > 0) {
						int x = (int)((44f - timer) / 44f * tex2d.width);
						Color pixel = tex2d.GetPixel(x, y); 
						float finalValue = (n == 0)? GetRYValue(pixel): GetBYValue(pixel);
						
						AddSample(GetSineValue(1500f + finalValue * 3.1372549f));
					}
					sineT = Mathf.Repeat(sineT, Mathf.PI * 2);
					y--;
				}
			}
		}
		
		if(scanMode == SlowScanMode.Robot72 || scanMode == SlowScanMode.Robot24) {
			bool r24 = (scanMode == SlowScanMode.Robot24);
			
			for(int y = tex2d.height; y >= 0; y--) {
				// Sync pulse
				timer += 9f;
				while(timer > 0) {
					AddSample(GetSineValue(1200f));
				}
				// Sync porch
				timer += 3f;
				while(timer > 0) {
					AddSample(GetSineValue(1500f));
				}
				//Y scan
				float yScanTime = r24? 88f: 138f;
				timer += yScanTime;
				while(timer > 0) {
					int x = (int)((yScanTime - timer) / yScanTime * tex2d.width);
					Color pixel = tex2d.GetPixel(x, y);
					float yValue = GetYValue(pixel);
					
					AddSample(GetSineValue(1500f + yValue * 3.1372549f));
				}
				for(int n = 0; n < 2; n++) {
					//Separator pulse
					timer += 4.5f;
					float evenodd = (n == 0)? 1500f: 2300f;
					while(timer > 0) {
						AddSample(GetSineValue(evenodd));
					}
					//Porch
					timer += 1.5f;
					float porchhz = (n == 0)? 1900f: 1500f;
					while(timer > 0) {
						AddSample(GetSineValue(porchhz));
					}
					// R-Y or B-Y scan
					float rbyScanTime = r24? 44f: 69f;
					timer += rbyScanTime;
					while(timer > 0) {
						int x = (int)((rbyScanTime - timer) / rbyScanTime * tex2d.width);
						Color pixel = tex2d.GetPixel(x, y); 
						float finalValue = (n == 0)? GetRYValue(pixel): GetBYValue(pixel);
						
						AddSample(GetSineValue(1500f + finalValue * 3.1372549f));
					}
				}
				sineT = Mathf.Repeat(sineT, Mathf.PI * 2);
			}
		}
		
		if(scanMode == SlowScanMode.Scottie1 || scanMode == SlowScanMode.Scottie2|| scanMode == SlowScanMode.ScottieDX) {
			// Sync pulse
			timer += 9f;
			while(timer > 0) {
				AddSample(GetSineValue(1200f));
			}
			
			float ScanTime = 0;
			switch(scanMode) {
			case SlowScanMode.Scottie1:
				ScanTime = 138.240f;
				break;
			case SlowScanMode.Scottie2:
				ScanTime = 88.064f;
				break;
			case SlowScanMode.ScottieDX:
				ScanTime =  345.6f;
				break;
			}
			
			for(int y = tex2d.height; y >= 0; y--) {
				// Sync porch
				timer += 1.5f;
				while(timer > 0) {
					AddSample(GetSineValue(1500f));
				}
				//Green scan
				timer += ScanTime;
				while(timer > 0) {
					int x = (int)((ScanTime - timer) / ScanTime * tex2d.width);
					float g = tex2d.GetPixel(x, y).g;
					
					AddSample(GetSineValue(1500f + g * 800f));
				}
				//Separator pulse
				timer += 1.5f;
				while(timer > 0) {
					AddSample(GetSineValue(1500f));
				}
				//Blue scan
				timer += ScanTime;
				while(timer > 0) {
					int x = (int)((ScanTime - timer) / ScanTime * tex2d.width);
					float b = tex2d.GetPixel(x, y).b;
					
					AddSample(GetSineValue(1500f + b * 800f));
				}
				// Sync pulse
				timer += 9f;
				while(timer > 0) {
					AddSample(GetSineValue(1200f));
				}
				//Porch
				timer += 1.5f;
				while(timer > 0) {
					AddSample(GetSineValue(1500f));
				}
				//Red scan
				timer += ScanTime;
				while(timer > 0) {
					int x = (int)((ScanTime - timer) / ScanTime * tex2d.width);
					float r = tex2d.GetPixel(x, y).r;
					
					AddSample(GetSineValue(1500f + r * 800f));
				}
				sineT = Mathf.Repeat(sineT, Mathf.PI * 2);
			}
		}
		
		clip.SetData(clipsamples, 0);
		return clip;
	}
	
	float GetYValue(Color pixel) {
		return 16.0f + (.003906f * ((65.738f * pixel.r * 255f) + (129.057f * pixel.g * 255f) + (25.064f * pixel.b * 255f)));
		//float kr = 0.299f; float kb = 0.114f;
		//return kr * pixel.r + (1 - kr - kb) * pixel.g + kb + pixel.b;
	}
	float GetRYValue(Color pixel) {
		return 128.0f + (.003906f * ((112.439f * pixel.r * 255f) + (-94.154f * pixel.g * 255f) + (-18.285f * pixel.b * 255f)));
	}
	float GetBYValue(Color pixel) {
		return 128.0f + (.003906f * ((-37.945f * pixel.r * 255f) + (-74.494f * pixel.g * 255f) + (112.439f * pixel.b * 255f)));
	}
	
	Texture2D ReadyTexture(Camera cam) {
		RenderTexture rt = new RenderTexture((int)res.x, (int)res.y, 16);
		cam.targetTexture = rt;
		cam.Render();
		cam.targetTexture = null;
		
		RenderTexture.active = rt;
		Texture2D tex2d = new Texture2D((int)res.x, (int)res.y);
		tex2d.ReadPixels(new Rect(0, 0, (int)res.x, (int)res.y), 0, 0);	
		tex2d.Apply();
		RenderTexture.active = null;
		
		return tex2d;
	}
	
	void AddSample(float val) {
		clipsamples[i] = val;
		i++;
		timer -= step;
	}
	
	float GetSineValue(float hz) {
		sineT += 2f * Mathf.PI * hz / sampleRate;
		return Mathf.Sin (sineT);
	}
}