namespace SpaceCalculator.Models;

public static class SpaceCalculations
{
    /// <summary>
    /// Computes delta-v from Tsiolkovsky rocket equation.
    /// </summary>
    public static double ComputeDeltaV(double initialMass, double finalMass, double exhaustVelocity)
    {
        if (initialMass <= 0 || finalMass <= 0 || exhaustVelocity <= 0)
        {
            throw new ArgumentException("Masses and exhaust velocity must be positive.");
        }

        if (finalMass >= initialMass)
        {
            throw new ArgumentException("Final mass must be lower than initial mass.");
        }

        return exhaustVelocity * Math.Log(initialMass / finalMass);
    }

    /// <summary>
    /// Circular orbital velocity at a radius in km given mu (km^3/s^2).
    /// </summary>
    public static double OrbitalVelocity(double gravitationalParameter, double radiusKm)
    {
        if (gravitationalParameter <= 0 || radiusKm <= 0)
        {
            throw new ArgumentException("μ and radius must be positive.");
        }

        return Math.Sqrt(gravitationalParameter / radiusKm);
    }

    /// <summary>
    /// Escape velocity at a radius in km given mu (km^3/s^2).
    /// </summary>
    public static double EscapeVelocity(double gravitationalParameter, double radiusKm)
    {
        return Math.Sqrt(2 * gravitationalParameter / radiusKm);
    }

    /// <summary>
    /// Hohmann transfer delta-v values in km/s for initial and final circular orbits.
    /// </summary>
    public static (double dv1, double dv2, double total) HohmannTransferDeltaV(double mu, double r1, double r2)
    {
        if (mu <= 0 || r1 <= 0 || r2 <= 0)
        {
            throw new ArgumentException("μ and radii must be positive.");
        }

        var sqrtMu = Math.Sqrt(mu);
        var dv1 = Math.Abs(sqrtMu * (Math.Sqrt(2 * r2 / (r1 + r2)) - Math.Sqrt(1 / r1)));
        var dv2 = Math.Abs(sqrtMu * (Math.Sqrt(2 * r1 / (r1 + r2)) - Math.Sqrt(1 / r2)));
        return (dv1, dv2, dv1 + dv2);
    }

    /// <summary>
    /// Surface gravity in m/s^2 given mu (km^3/s^2) and radius in km.
    /// </summary>
    public static double SurfaceGravity(double mu, double radiusKm)
    {
        if (mu <= 0 || radiusKm <= 0)
        {
            throw new ArgumentException("μ and radius must be positive.");
        }

        var muMeters = mu * 1e9; // convert km^3/s^2 to m^3/s^2
        var radiusMeters = radiusKm * 1000;
        return muMeters / (radiusMeters * radiusMeters);
    }
}
