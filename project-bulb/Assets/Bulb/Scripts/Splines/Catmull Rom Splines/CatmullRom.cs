using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Splines.CatmullRom
{
    public static class CatmullRom
    {
        /**
     * This method will calculate the Catmull-Rom interpolation curve, returning
     * it as a list of Vector3s.  This method in particular
     * adds the first and last control points which are not visible, but required
     * for calculating the spline.
     *
     * @param Vector3inates The list of original straight line points to calculate
     * an interpolation from.
     * @param pointsPerSegment The integer number of equally spaced points to
     * return along each curve.  The actual distance between each
     * point will depend on the spacing between the control points.
     * @return The list of interpolated Vector3inates.
     * @param curveType Chordal (stiff), Uniform(floppy), or Centripetal(medium)
     * @throws gov.ca.water.shapelite.analysis.CatmullRomException if
     * pointsPerSegment is less than 2.
     */
        public static List<Vector3> Interpolate(List<Vector3> coords, int pointsPerSegment, CatmullRomType curveType)
        {
            List<Vector3> pointsOnSpline = new List<Vector3>();
            foreach (Vector3 c in coords)
            {
                pointsOnSpline.Add(c);
            }
            if (pointsPerSegment < 2)
            {
                Debug.LogError("The pointsPerSegment parameter must be greater than 2, since 2 points is just the linear segment.");
                return null;
            }

            // Cannot interpolate curves given only two points.  Two points
            // is best represented as a simple line segment.
            if (pointsOnSpline.Count < 3)
            {
                return pointsOnSpline;
            }

            // Test whether the shape is open or closed by checking to see if
            // the first point intersects with the last point. 
            bool isClosed = pointsOnSpline[0] == pointsOnSpline[pointsOnSpline.Count - 1];
            if (isClosed)
            {
                // Use the second and second from last points as control points.
                // get the second point.
                Vector3 p2 = pointsOnSpline[1];
                // get the point before the last point
                Vector3 pn1 = pointsOnSpline[pointsOnSpline.Count - 2];

                // insert the second from the last point as the first point in the list
                // because when the shape is closed it keeps wrapping around to
                // the second point.
                pointsOnSpline.Insert(0, pn1);
                // add the second point to the end.
                pointsOnSpline.Add(p2);
            }
            else
            {
                // The shape is open, so use control points that simply extend
                // the first and last segments

                // Get the change in x and y between the first and second coordinates.
                Vector3 delta = pointsOnSpline[1] - pointsOnSpline[0];

                // Then using the change, extrapolate backwards to find a control point.
                Vector3 start = pointsOnSpline[0] - delta;

                // Repeat for the end control point.
                int n = pointsOnSpline.Count - 1;
                delta = pointsOnSpline[n] - pointsOnSpline[n - 1];
                Vector3 end = pointsOnSpline[n] + delta;

                // insert the start control point at the start of the vertices list.
                pointsOnSpline.Insert(0, start);

                // append the end control ponit to the end of the vertices list.
                pointsOnSpline.Add(end);
            }

            // Dimension a result list of Vector3inates. 
            List<Vector3> result = new List<Vector3>();
            // When looping, remember that each cycle requires 4 points, starting
            // with i and ending with i+3.  So we don't loop through all the points.
            for (int i = 0; i < pointsOnSpline.Count - 3; i++)
            {
                // Actually calculate the Catmull-Rom curve for one segment.
                List<Vector3> points = InterpolateSegment(pointsOnSpline, i, pointsPerSegment, curveType);
                // Since the middle points are added twice, once for each bordering
                // segment, we only add the 0 index result point for the first
                // segment.  Otherwise we will have duplicate points.
                if (result.Count > 0)
                {
                    points.RemoveAt(0);
                }

                // Add the Vector3inates for the segment to the result list.
                result.AddRange(points);
            }
            return result;
        }

        /**
         * Given a list of control points, this will create a list of pointsPerSegment
         * points spaced uniformly along the resulting Catmull-Rom curve.
         *
         * @param points The list of control points, leading and ending with a 
         * Vector3inate that is only used for controling the spline and is not visualized.
         * @param index The index of control point p0, where p0, p1, p2, and p3 are
         * used in order to create a curve between p1 and p2.
         * @param pointsPerSegment The total number of uniformly spaced interpolated
         * points to calculate for each segment. The larger this number, the
         * smoother the resulting curve.
         * @param curveType Clarifies whether the curve should use uniform, chordal
         * or centripetal curve types. Uniform can produce loops, chordal can
         * produce large distortions from the original lines, and centripetal is an
         * optimal balance without spaces.
         * @return the list of Vector3inates that define the CatmullRom curve
         * between the points defined by index+1 and index+2.
         */
        private static List<Vector3> InterpolateSegment(List<Vector3> points, int index, int pointsPerSegment, CatmullRomType curveType)
        {
            List<Vector3> result = new List<Vector3>();
            Vector3[] controlPointsInSegment = new Vector3[4];
            float[] time = new float[4];
            for (int i = 0; i < 4; i++)
            {
                controlPointsInSegment[i] = points[index + i];
                time[i] = i;
            }

            float tstart = 1;
            float tend = 2;
            if (curveType != CatmullRomType.Uniform)
            {
                float total = 0;
                for (int i = 1; i < 4; i++)
                {
                    Vector3 delta = controlPointsInSegment[i] - controlPointsInSegment[i - 1];
                    if (curveType == CatmullRomType.Centripetal)
                    {
                        total += Mathf.Pow(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z, 0.25f);
                    }
                    else
                    {
                        total += Mathf.Pow(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z, 0.5f);
                    }
                    time[i] = total;
                }
                tstart = time[1];
                tend = time[2];
            }
            int segments = pointsPerSegment - 1;
            result.Add(points[index + 1]);
            for (int i = 1; i < segments; i++)
            {
                float xi = Interpolate(controlPointsInSegment.Select(p => p.x).ToArray(), time, tstart + (i * (tend - tstart)) / segments);
                float yi = Interpolate(controlPointsInSegment.Select(p => p.y).ToArray(), time, tstart + (i * (tend - tstart)) / segments);
                float zi = Interpolate(controlPointsInSegment.Select(p => p.z).ToArray(), time, tstart + (i * (tend - tstart)) / segments);
                result.Add(new Vector3(xi, yi, zi));
            }
            result.Add(points[index + 2]);
            return result;
        }

        /**
         * Unlike the other implementation here, which uses the default "uniform"
         * treatment of t, this computation is used to calculate the same values but
         * introduces the ability to "parameterize" the t values used in the
         * calculation. This is based on Figure 3 from
         * http://www.cemyuksel.com/research/catmullrom_param/catmullrom.pdf
         *
         * @param p An array of float values of length 4, where interpolation
         * occurs from p1 to p2.
         * @param time An array of time measures of length 4, corresponding to each
         * p value.
         * @param t the actual interpolation ratio from 0 to 1 representing the
         * position between p1 and p2 to interpolate the value.
         * @return
         */
        private static float Interpolate(float[] p, float[] time, float t)
        {
            float L01 = p[0] * (time[1] - t) / (time[1] - time[0]) + p[1] * (t - time[0]) / (time[1] - time[0]);
            float L12 = p[1] * (time[2] - t) / (time[2] - time[1]) + p[2] * (t - time[1]) / (time[2] - time[1]);
            float L23 = p[2] * (time[3] - t) / (time[3] - time[2]) + p[3] * (t - time[2]) / (time[3] - time[2]);
            float L012 = L01 * (time[2] - t) / (time[2] - time[0]) + L12 * (t - time[0]) / (time[2] - time[0]);
            float L123 = L12 * (time[3] - t) / (time[3] - time[1]) + L23 * (t - time[1]) / (time[3] - time[1]);
            float C12 = L012 * (time[2] - t) / (time[2] - time[1]) + L123 * (t - time[1]) / (time[2] - time[1]);
            return C12;
        }
    }

    public enum CatmullRomType
    {
        Chordal,
        Uniform,
        Centripetal
    }
}