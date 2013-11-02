using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BspViewer
{
    class BezierPatch
    {
        private int _level;
        public vertex[] _vertices;
        public int[] _indices;
        //private int[] _trianglesPerRow;
        //private int[] _rowIndices;

        public vertex[] _controls;
        public BezierPatch(vertex[] controls, int Level)
        {
            _controls = controls;
            tesselatePatch(Level);

        }

        private void tesselatePatch(int L)
        {
            _level = L;

            int L1 = L + 1;

            _vertices = new vertex[L1 * L1];

            //Compute vertices

            for (int i = 0; i <= L; ++i)
            {
                double a = (double)i / L;
                double b = 1 - a;

                _vertices[i] =
                    _controls[0] * (b * b) +
                    _controls[3] * (2 * b * a) +
                    _controls[6] * (a * a);

            }

            for (int i = 1; i <= L; ++i)
            {
                double a = (double)i / L;
                double b = 1 - a;


                vertex[] temp = new vertex[3];


                for (int j = 0; j < 3; ++j)
                {
                    int k = 3 * j;
                    temp[j] =
                        _controls[k + 0] * (b * b) +
                        _controls[k + 1] * (2 * b * a) +
                        _controls[k + 2] * (a * a);


                }

                for (int j = 0; j < L; ++j)
                {
                    _vertices[i * L1 + j] =
                        temp[0] * (b * b) +
                        temp[1] * (2 * b * a) +
                        temp[2] * (a * a);
                }
            }


            //indices
            _indices = new int[L * L1 * 2];

            for (int row = 0; row < L; ++row)
            {
                for (int col = 0; col <= L; ++col)
                {
                    _indices[(row * (L + 1) + col) * 2 + 1] = row * L1 + col;
                    _indices[(row * (L + 1) + col) * 2] = (row + 1) * L1 + col;
                }
            }

            /*
            _trianglesPerRow = new int[L];
            for (int row = 0; row < L; ++row)
            {
                _trianglesPerRow[row] = 2 * L1;
                _rowIndices[row] = _indices[row * 2 * L1];
            }

            */

        }
    }
}
