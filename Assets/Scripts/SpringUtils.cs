using System;
using UnityEngine;

public static class SpringUtils
{
    public class SpringMotionParams
    {
        public float posPosCoef, posVelCoef;
        public float velPosCoef, velVelCoef;
    };
    
    public static void CalcDampedSpringMotionParams(
        SpringMotionParams        springOutParams,       // motion parameters result
        float	                        deltaTime,        // time step to advance
        float	                        angularFrequency, // angular frequency of motion
        float	                        dampingRatio)     // damping ratio of motion
    {
        const float epsilon = 0.0001f;

        // force values into legal range
        if (dampingRatio     < 0.0f) dampingRatio     = 0.0f;
        if (angularFrequency < 0.0f) angularFrequency = 0.0f;

        // if there is no angular frequency, the spring will not move and we can
        // return identity
        if ( angularFrequency < epsilon )
        {
            springOutParams.posPosCoef = 1.0f; springOutParams.posVelCoef = 0.0f;
            springOutParams.velPosCoef = 0.0f; springOutParams.velVelCoef = 1.0f;
            return;
        }

        if (dampingRatio > 1.0f + epsilon)
        {
            // over-damped
            float za = -angularFrequency * dampingRatio;
            float zb = angularFrequency * Mathf.Sqrt(dampingRatio*dampingRatio - 1.0f);
            float z1 = za - zb;
            float z2 = za + zb;

            float e1 = Mathf.Exp( z1 * deltaTime );
            float e2 = Mathf.Exp( z2 * deltaTime );

            float invTwoZb = 1.0f / (2.0f*zb); // = 1 / (z2 - z1)
                
            float e1_Over_TwoZb = e1*invTwoZb;
            float e2_Over_TwoZb = e2*invTwoZb;

            float z1e1_Over_TwoZb = z1*e1_Over_TwoZb;
            float z2e2_Over_TwoZb = z2*e2_Over_TwoZb;

            springOutParams.posPosCoef =  e1_Over_TwoZb*z2 - z2e2_Over_TwoZb + e2;
            springOutParams.posVelCoef = -e1_Over_TwoZb    + e2_Over_TwoZb;

            springOutParams.velPosCoef = (z1e1_Over_TwoZb - z2e2_Over_TwoZb + e2)*z2;
            springOutParams.velVelCoef = -z1e1_Over_TwoZb + z2e2_Over_TwoZb;
        }
        else if (dampingRatio < 1.0f - epsilon)
        {
            // under-damped
            float omegaZeta = angularFrequency * dampingRatio;
            float alpha     = angularFrequency * Mathf.Sqrt(1.0f - dampingRatio*dampingRatio);

            float expTerm = Mathf.Exp( -omegaZeta * deltaTime );
            float cosTerm = Mathf.Cos( alpha * deltaTime );
            float sinTerm = Mathf.Sin( alpha * deltaTime );
                
            float invAlpha = 1.0f / alpha;

            float expSin = expTerm*sinTerm;
            float expCos = expTerm*cosTerm;
            float expOmegaZetaSin_Over_Alpha = expTerm*omegaZeta*sinTerm*invAlpha;

            springOutParams.posPosCoef = expCos + expOmegaZetaSin_Over_Alpha;
            springOutParams.posVelCoef = expSin*invAlpha;

            springOutParams.velPosCoef = -expSin*alpha - omegaZeta*expOmegaZetaSin_Over_Alpha;
            springOutParams.velVelCoef =  expCos - expOmegaZetaSin_Over_Alpha;
        }
        else
        {
            // critically damped
            float expTerm     = Mathf.Exp( -angularFrequency*deltaTime );
            float timeExp     = deltaTime*expTerm;
            float timeExpFreq = timeExp*angularFrequency;

            springOutParams.posPosCoef = timeExpFreq + expTerm;
            springOutParams.posVelCoef = timeExp;

            springOutParams.velPosCoef = -angularFrequency*timeExpFreq;
            springOutParams.velVelCoef = -timeExpFreq + expTerm;
        }
    }
    
    public static void UpdateDampedSpringMotion
    (
        ref float position,
        ref float velocity,
        float equilibriumPosition,
        in SpringMotionParams springParams
    )
    {
        float oldPosition = position - equilibriumPosition;
        float oldVelocity = velocity;

        position = oldPosition * springParams.posPosCoef + oldVelocity * springParams.posVelCoef + equilibriumPosition;
        velocity = oldPosition * springParams.velPosCoef + oldVelocity * springParams.velVelCoef;
    }
}
