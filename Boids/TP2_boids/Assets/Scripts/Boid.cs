﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class Boid : MonoBehaviour
{

    public BoidManager manager;

    private Vector3 velocity = Vector3.one;

    private Vector3 lastTickVelocity = Vector3.one;



    void Update() {


        //Get closests neighbours for 
        List<Boid> attractionBoids = GetNeighbours(manager.boids, manager.attractionDistance);
        Vector3 attraction = GetAttraction(attractionBoids);

        List<Boid> alignemntBoids = GetNeighbours(attractionBoids, manager.aligmentDistance);
        Vector3 align = GetAligment(alignemntBoids);

        List<Boid> repulstionBoids = GetNeighbours(alignemntBoids, manager.repulsionDistance);
        Vector3 repulsion = GetRepulsion(repulstionBoids);

        velocity = (attraction + align + repulsion + SomeRandom(lastTickVelocity)) / 4;


        // If close to borders, move away
        float borderX = manager.cube.transform.localScale.x / 2 * 0.9f;
        float borderY = manager.cube.transform.localScale.y / 2 * 0.9f;
        float borderZ = manager.cube.transform.localScale.z / 2 * 0.9f;

        if (transform.position.x < -borderX && velocity.x < 0) {
            velocity.x = -velocity.x * UnityEngine.Random.Range(0.1f, 1.0f);
        }
        if (transform.position.y < -borderY && velocity.y < 0) {
            velocity.y = -velocity.y * UnityEngine.Random.Range(0.1f, 1.0f);
        }
        if (transform.position.z < -borderZ && velocity.z < 0) {
            velocity.z = -velocity.z * UnityEngine.Random.Range(0.1f, 1.0f);
        }

        if (transform.position.x > borderX && velocity.x > 0) {
            velocity.x = -velocity.x * UnityEngine.Random.Range(0.1f, 1.0f);
        }
        if (transform.position.y > borderY && velocity.y > 0) {
            velocity.y = -velocity.y * UnityEngine.Random.Range(0.1f, 1.0f);
        }
        if (transform.position.z > borderZ && velocity.z > 0) {
            velocity.z = -velocity.z * UnityEngine.Random.Range(0.1f, 1.0f);
        }

        velocity = velocity.normalized;
        lastTickVelocity = velocity;

        // Apply movement
        Vector3 nextPos = transform.position + Time.deltaTime * manager.boidSpeed * velocity;
        transform.rotation = Quaternion.LookRotation(transform.position - nextPos);
        transform.position = nextPos;
    
    }

    

    private Vector3 SomeRandom(Vector3 lastTickVelocity) {

        float angleX = UnityEngine.Random.Range(-manager.randomAngle, manager.randomAngle);
        float angleY = UnityEngine.Random.Range(-manager.randomAngle, manager.randomAngle);
        float angleZ = UnityEngine.Random.Range(-manager.randomAngle, manager.randomAngle);

        Quaternion qt = Quaternion.Euler(angleX, angleY, angleZ);

        return qt * lastTickVelocity;
    }



    // Compute the normalized vector from current position to average position of neighbours
    private Vector3 GetAttraction(List<Boid> attractionBoids) {

        // If no neighbours, return current velocity
        if (attractionBoids.Count == 0) {
            return lastTickVelocity;
        }

        //Go towards the center of neighbours
        Vector3 centerPosition = Vector3.zero;

        foreach (Boid boid in attractionBoids) {
            centerPosition += boid.transform.position;
        }

        centerPosition /= attractionBoids.Count;

        Vector3 attraction = (centerPosition - transform.position).normalized;

        if (manager.target != null) {
            attraction += (manager.target.transform.position - transform.position).normalized;
            attraction /= 2;
        }

        // Returning the normalized vector from current position to centerPosition
        return attraction;

    }

    //Compute 
    private Vector3 GetAligment(List<Boid> alignemntBoids) {
        if (alignemntBoids.Count == 0) {
            return lastTickVelocity;
        }

        Vector3 averageDirection = Vector3.zero;

        foreach (Boid boid in alignemntBoids) {
            averageDirection += boid.lastTickVelocity;
        }

        averageDirection /= alignemntBoids.Count;

        return averageDirection.normalized;
    }

    //Compute the normalized vector going away from the average position of neighbours
    private Vector3 GetRepulsion(List<Boid> repulstionBoids) {
        if (repulstionBoids.Count == 0) {
            return lastTickVelocity;
        }

        Vector3 centerPosition = Vector3.zero;

        foreach (Boid boid in repulstionBoids) {
            centerPosition += boid.transform.position;
        }

        centerPosition /= repulstionBoids.Count;

        // Returning the normalized vector form centerPosition to current position
        // Meaning we return a vector going in the other direction from the center position of neighbours

        Vector3 positionToCenterPosition = (transform.position - centerPosition).normalized;

        if (manager.obstacle != null) {
            if (Vector3.Distance(manager.obstacle.position, transform.position) > manager.obstacleRange) {
                return positionToCenterPosition;
            }

            // Else
            Vector3 obstacleToPosition = (transform.position - manager.obstacle.position).normalized;
            return (positionToCenterPosition + obstacleToPosition) / 2;
        }
        // Else
        return positionToCenterPosition;

    }

    private List<Boid> GetNeighbours(List<Boid> boidList, float distance) {
        List<Boid> closestBoids = new List<Boid>();

        foreach (Boid boid in manager.boids) {

            //Not taking itself
            if(boid == this) {
                continue;
            }

            if (Vector3.Distance(transform.position, boid.transform.position) <= distance) {
                closestBoids.Add(boid);
            }
        }

        return closestBoids;
    }


    internal void SetVelocity(Vector3 vel) {
        velocity = vel;
        lastTickVelocity = vel;
    }

    internal void SetController(BoidManager boidManager) {
        manager = boidManager;
    }
}
