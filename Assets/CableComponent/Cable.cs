using UnityEngine;
using System;
using System.Collections;

public class Cable : MonoBehaviour
{
	[SerializeField] private Transform EndPointTransform;
	[SerializeField] private Material material;

	[Header("Cable configuration")]
	[SerializeField] private float totalLength = 250f;
	[SerializeField] private int totalSegments = 50;
	[SerializeField] private float segmentsPerUnit = 8f;
	[SerializeField] private float cableWidth = 0.075f;
	private int segments = 0;

	[Header("Solver configuration")]
	[SerializeField] private int verletIters = 1;
	[SerializeField] private int solverIters = 1;

	[Range(0, 1)]
	[SerializeField] private float speedround = 1f;

	private LineRenderer line;
	private CableSegment[] points;

	void Start()
	{
		InitCableParticles();
		InitLineRenderer();
	}

	/// <summary>
	/// Инициализировать частицы кабеля<br></br><br></br>
	/// Создает частицы кабеля по длине кабеля и связывает начальную и конечную подсказки с соответствующими игровыми объектами.
	/// </summary>
	void InitCableParticles()
	{
		// Подсчитываем количество используемых сегментов, если таковы не заданы
		if (totalSegments > 0)
			segments = totalSegments;
		else
			segments = Mathf.CeilToInt(totalLength * segmentsPerUnit);

		Vector3 cableDirection = (EndPointTransform.position - transform.position).normalized; // Делает вектор длинной 1 юнит
		float initialSegmentLength = totalLength / segments; // Длина одного сегмента
		points = new CableSegment[segments + 1]; // Создаёт частицы кабеля (пока пустые)

		for (int pointIdx = 0; pointIdx <= segments; pointIdx++)
		{
			// Устанавливает частица координаты
			Vector3 initialPosition = transform.position + (cableDirection * (initialSegmentLength * pointIdx));
			points[pointIdx] = new CableSegment(initialPosition);
		}

		// Связывает начальные и конечные частицы с соответствующими игровыми объектами.
		CableSegment start = points[0];
		CableSegment end = points[segments];
		start.Attach(this.transform);
		end.Attach(EndPointTransform.transform);
	}

	[Obsolete]
	void InitLineRenderer()
	{
		line = this.gameObject.AddComponent<LineRenderer>();
		line.SetWidth(cableWidth, cableWidth);
		line.SetVertexCount(segments + 1);
		line.material = material;
		line.GetComponent<Renderer>().enabled = true;
	}

	void Update()
	{
		RenderCable();
	}

	/**
	 * Render Cable
	 * Обновить положение каждой частицы
	 */
	void RenderCable()
	{
		for (int pointIdx = 0; pointIdx < segments + 1; pointIdx++)
		{
			line.SetPosition(pointIdx, points[pointIdx].CurrentPosition);
		}
	}

	void FixedUpdate()
	{
		for (int verletIdx = 0; verletIdx < verletIters; verletIdx++)
		{
			VerletIntegrate();
			SolveConstraints();
		}
	}

	void VerletIntegrate()
	{
		Vector3 gravityDisplacement = Time.fixedDeltaTime * Time.fixedDeltaTime * Physics.gravity * speedround;
		foreach (CableSegment particle in points)
		{
			particle.UpdateVerlet(gravityDisplacement);
		}
	}

	void SolveConstraints()
	{
		// For each solver iteration..
		for (int iterationIdx = 0; iterationIdx < solverIters; iterationIdx++)
		{
			DistanceConstraint();
		}
	}

	void DistanceConstraint()
	{
		float segmentLength = totalLength / segments;
		for (int SegIdx = 0; SegIdx < segments; SegIdx++)
		{
			CableSegment segmentA = points[SegIdx];
			CableSegment segmentB = points[SegIdx + 1];

			// Solve for this pair of particles
			DistanceConstraint(segmentA, segmentB, segmentLength);
		}
	}

	/** 
	 * Основные ограничения, которые удерживают частицы кабеля связанными
	 */
	void DistanceConstraint(CableSegment segmentA, CableSegment segmentB, float segmentLength)
	{
		// Найдём текущий вектор между частицами
		Vector3 delta = segmentB.CurrentPosition - segmentA.CurrentPosition;
		
		float currentDistance = delta.magnitude; // Длина вектора
		float deviation = (currentDistance - segmentLength) / currentDistance;

		// Перемещаем только свободные частицы, чтобы удовлетворить ограничения
		if (segmentA.CheckFree() && segmentB.CheckFree())
		{
			segmentA.CurrentPosition += deviation * 0.5f * delta;
			segmentB.CurrentPosition -= deviation * 0.5f * delta;
		}
		else if (segmentA.CheckFree())
		{
			segmentA.CurrentPosition += deviation * delta;
		}
		else if (segmentB.CheckFree())
		{
			segmentB.CurrentPosition -= deviation * delta;
		}
	}
}
