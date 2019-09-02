using System.Collections;
using UnityEngine;

//A simple blobshadow script that rotates and scales based upon the surface normal and distance.
//My aim was to have a cheap solution that runs flawless on mobile cpu's. If you have more cpu power at your disposal i recommend replacing this with a unity projector or a directional light with shadow casting for more accurate results.
public class SimpleBlobShadow : MonoBehaviour {

	public Transform FollowBone;
	public float BlobShadowSize = 1;
	public float DistanceScale = 2f; //the size multiplier of the blobshadow relative to the distance from the floor
	public float Yoffset = 0; //the offset of the blobshadow
	public LayerMask GroundLayerMask;
	public bool followTerrainRotation = true;
	private float rayDist = 10f; //raycast distance

	void Update(){
		if (FollowBone != null) {

			//raycast down
			RaycastHit hit;
			if (Physics.Raycast (FollowBone.transform.position, Vector3.down * rayDist, out hit, rayDist, GroundLayerMask)) {

				//show blobshadow if we've hit something
				GetComponent<MeshRenderer>().enabled = true;

				//set position
				setPosition (hit);

				//set scale
				setScale(FollowBone.transform.position.y - hit.point.y);

				//set blobshadow rotation to hit normal
				if (followTerrainRotation) {
					setRotation(hit.normal);
				}

			} else {
				
				//hide blobshadow
				GetComponent<MeshRenderer>().enabled = false;
			}
		}
	}

	//set shadow position
	void setPosition(RaycastHit hit){
		transform.position = hit.point + Vector3.up * Yoffset;
	}

	//set shadow rotation to floor angle
	void setRotation(Vector3 normal){
		transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
	}

	//set the scale of the blobshadow
	void setScale(float floorDistance){
		float scaleMultiplier = floorDistance / DistanceScale;
		float size = BlobShadowSize + scaleMultiplier;
		transform.localScale = new Vector3 (size, size, size);
	}
}
