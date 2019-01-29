using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class ChunkIterator : IEnumerable<ChunkIterator.Chunk>
    {
        public struct Chunk
        {
            public Vector2Int Start;
            public Vector2Int End;

            public Vector2Int Size
            {
                get { return End - Start;}
            }

            public int Width
            {
                get { return End.x - Start.x; }
            }

            public int Height
            {
                get { return End.y - Start.y; }
            }
        }

        private readonly int _width;
        private readonly int _height;
        private readonly int _chunkWidth;
        private readonly int _chunkHeight;

        public ChunkIterator(int width, int height, int chunkWidth, int chunkHeight)
        {
            _width = width;
            _height = height;
            _chunkHeight = chunkHeight;
            _chunkWidth = chunkWidth;
        }

        public IEnumerator<Chunk> GetEnumerator()
        {
            var xSteps = _width / _chunkWidth;
            var ySteps = _height / _chunkHeight;

            if (_width - xSteps * _chunkWidth > 0)
                xSteps += 1;

            if (_height - ySteps * _chunkHeight > 0)
                ySteps += 1;

            for (var i = 0; i < ySteps; i++)
            {
                var y1 = i * _chunkHeight;
                var y2 = Mathf.Min((i + 1) * _chunkHeight, _height);

                for (var j = 0; j < xSteps; j++)
                {
                    var x1 = j * _chunkWidth;
                    var x2 = Mathf.Min((j + 1) * _chunkWidth, _width);
                    yield return new Chunk
                    {
                        Start = new Vector2Int(x1, y1),
                        End = new Vector2Int(x2, y2)
                    };
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
