using System.Collections;
using UnityEngine;

public class CableSegment
{
	private Rigidbody attachmentTo_rb = null;
	private Transform attachmentTo = null;
	private Vector3 position, oldPosition;
	public Vector3 CurrentPosition {
		get { return position; }
		set { position = value; }
	}
	public Vector3 Velocity {
		get { return (position - oldPosition); }
	}
	public CableSegment(Vector3 newPosition)
	{
		oldPosition = position = newPosition;
	}
	public void UpdateVerlet(Vector3 gravityDisplacement)
	{
		if (this.CheckAttachment())
		{
			if (attachmentTo_rb == null) {
				this.UpdatePosition(attachmentTo.position);		
			}
			else
			{
				switch (attachmentTo_rb.interpolation) 
				{
					case RigidbodyInterpolation.Interpolate:
						this.UpdatePosition(attachmentTo_rb.position + (attachmentTo_rb.velocity * Time.fixedDeltaTime) / 2);
						break;
					case RigidbodyInterpolation.None:
					default:
						this.UpdatePosition(attachmentTo_rb.position + attachmentTo_rb.velocity * Time.fixedDeltaTime);
						break;
				}
			}
		}
		else 
		{
			Vector3 newPosition = this.CurrentPosition + this.Velocity + gravityDisplacement;
			this.UpdatePosition(newPosition);
		}
	}
	public void UpdatePosition(Vector3 newPosition) 
	{
		oldPosition = position;
		position = newPosition;
	}
	public void Attach(Transform to)
	{
		attachmentTo = to;
		attachmentTo_rb = to.GetComponent<Rigidbody>();
		oldPosition = position = attachmentTo.position;
	}
	public void ClearAttachment()
	{
		attachmentTo = null;
		attachmentTo_rb = null;
	}
	public bool CheckFree()
	{
		return (attachmentTo == null);
	}
	public bool CheckAttachment()
	{
		return (attachmentTo != null);
	}
}