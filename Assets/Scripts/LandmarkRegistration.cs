using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using static System.Numerics.Quaternion;
using System.Linq;

public class LandmarkRegistration : MonoBehaviour
{
    public List<Vector3> from;
    public List<Vector3> to;
    //public Transform base_pos;
    public Transform cam_pos;
    public Transform from_list;
    public Transform to_list;
    public GameObject ur_base;
    public GameObject points;
    public GameObject real_eelink;
    int from_num = 0;
    int to_num = 0;
    // Start is called before the first frame update
    void Start()
    {
        //from = new List<Vector3> {
        //    new Vector3(1.0f, 1.0f, 1.0f),
        //    new Vector3(2.0f, 2.0f, 2.0f),
        //    new Vector3(3.0f, 3.0f, 3.0f) };
        //to = new List<Vector3> {
        //    new Vector3(1.1f, 1.0f, 1.0f),
        //    new Vector3(2.1f, 2.0f, 2.0f),
        //    new Vector3(3.1f, 3.0f, 3.0f)};
        from = new List<Vector3>();
        to = new List<Vector3>();
    }

    void create_point(int types)
    {
        //GameObject point = (GameObject)Resources.Load("Prefabs/Point");
        Vector3 point_pos = cam_pos.position;
        //point_pos.z -= 0.5f;
        GameObject point = Instantiate(points, point_pos, Quaternion.identity) as GameObject;


        if (types == 0)
        {
            point.name = from_num.ToString();
            point.GetComponent<Renderer>().material.color = Color.yellow;
            point.transform.parent = from_list.transform;
            from_num += 1;
        }
        else
        {
            point.name = to_num.ToString();
            point.GetComponent<Renderer>().material.color = Color.green;
            point.transform.parent = to_list.transform;
            to_num += 1;
        }
        //Instantiate(point);
    }

    public void from_place()
    {
        create_point(0);
    }

    public void to_place()
    {
        create_point(1);
    }

    public void clear_to()
    {
        for (int i = 0; i < to_list.transform.childCount; i++)
        {
            Transform childTransform = to_list.transform.Find(i.ToString());
            GameObject.Destroy(childTransform.gameObject);
        }
        to_num = 0;
    }

    void get_transform()
    {
        from.Clear();
        to.Clear();
        for (int i = 0; i < from_list.transform.childCount; i++)
        {
            Transform childTransform = from_list.transform.Find(i.ToString());
            print(childTransform);
            Vector3 childPosition = childTransform.position;
            from.Add(childPosition);
        }
        for (int i = 0; i < to_list.transform.childCount; i++)
        {
            Transform childTransform = to_list.transform.Find(i.ToString());
            Vector3 childPosition = childTransform.position;
            to.Add(childPosition);
        }
    }

    void SVD_solve()
    {
        var sourceMatrix = DenseMatrix.OfRowVectors(from.Select(p => new DenseVector(new double[] { p.x, p.y, -p.z })));
        var targetMatrix = DenseMatrix.OfRowVectors(to.Select(p => new DenseVector(new double[] { p.x, p.y, -p.z })));

        // 计算质心
        var sourceCentroid = sourceMatrix.ColumnSums() / sourceMatrix.RowCount;
        var targetCentroid = targetMatrix.ColumnSums() / targetMatrix.RowCount;

        // 中心化点云
        var centeredSource = sourceMatrix.Subtract(DenseMatrix.Create(sourceMatrix.RowCount, sourceMatrix.ColumnCount, (i, j) => sourceCentroid[j]));
        var centeredTarget = targetMatrix.Subtract(DenseMatrix.Create(targetMatrix.RowCount, targetMatrix.ColumnCount, (i, j) => targetCentroid[j]));

        //print(centeredSource);
        //print(centeredTarget);

        // 计算协方差矩阵
        var covarianceMatrix = centeredSource.Transpose() * centeredTarget;

        // 奇异值分解
        var svd = covarianceMatrix.Svd(true);

        // 构建旋转矩阵
        var rotationMatrix = svd.VT.Transpose() * svd.U.Transpose();
        if (rotationMatrix.Determinant() < 0)
        {
            var V = svd.VT.Transpose();
            V[0, 1] = -V[0, 1];
            V[1, 1] = -V[1, 1];
            V[2, 1] = -V[2, 1];
            rotationMatrix = V * svd.U.Transpose();
        }

        // 构建平移矩阵
        var translationVector = targetCentroid - rotationMatrix * sourceCentroid;

        var Sz = DiagonalMatrix.CreateIdentity(3);
        Sz[2, 2] = -1;

        rotationMatrix = Sz * rotationMatrix * Sz;

        var translationMatrix = DenseMatrix.OfColumnVectors(new DenseVector(new double[] { translationVector[0], translationVector[1], -translationVector[2] }));
        var transformationMatrix = rotationMatrix.Append(translationMatrix);
        transformationMatrix = InsertRow(transformationMatrix, new double[] {0, 0, 0, 1}, 3);
        print(transformationMatrix);
        //print(translationMatrix);

        var base_pos = ur_base.transform;
        Vector3 position = base_pos.position;
        Quaternion rotation = base_pos.rotation;
        //Vector3 scale = base_pos.localScale;
        Vector3 scale = new Vector3(1,1,1);

        // 创建一个仿射变换矩阵
        Matrix4x4 transformMatrix = Matrix4x4.TRS(position, rotation, scale);
        var from_pose = DenseMatrix.Create(4, 4, (i, j) => transformMatrix[i, j]);

        //position = cam_pos.position;
        //rotation = cam_pos.rotation;
        //scale = cam_pos.localScale;
        //Matrix4x4 camMatrix = Matrix4x4.TRS(position, rotation, scale);
        //var cam_pose = DenseMatrix.Create(4, 4, (i, j) => camMatrix[i, j]);

        //var to_pose = from_pose * transformationMatrix.Inverse() * cam_pose;
        var to_pose = transformationMatrix * from_pose;
        print(to_pose);

        position = new Vector3((float)to_pose[0, 3], (float)to_pose[1, 3], (float)to_pose[2, 3]);
        System.Numerics.Matrix4x4 unityMatrix = new System.Numerics.Matrix4x4(
            (float)to_pose[0, 0], (float)to_pose[0, 1], (float)to_pose[0, 2], 0,
            (float)to_pose[1, 0], (float)to_pose[1, 1], (float)to_pose[1, 2], 0,
            (float)to_pose[2, 0], (float)to_pose[2, 1], (float)to_pose[2, 2], 0,
            0, 0, 0, 1
        );
        var systemQuaternion = CreateFromRotationMatrix(unityMatrix);
        print(systemQuaternion);
        Quaternion unityQuaternion = new Quaternion(
            systemQuaternion.X,
            systemQuaternion.Y,
            systemQuaternion.Z,
            -systemQuaternion.W
        );

        //print(position);
        //print(unityQuaternion);
        //cam_pos.position = position;
        //cam_pos.rotation = unityQuaternion;
        //this.GetComponent<RosSubscriberExample>().reset_joints();
        ur_base.SetActive(false);
        ur_base.transform.position = position;
        ur_base.transform.rotation = unityQuaternion;
        ur_base.SetActive(true);
        real_eelink.SetActive(true);
        

        //print(rotationMatrix);
        //print(translationVector);
    }

    Matrix<double> InsertRow(Matrix<double> matrix, double[] newRow, int index)
    {
        // 创建一个新的 DenseMatrix 用于存储更新后的矩阵
        var newMatrix = DenseMatrix.Create(matrix.RowCount + 1, matrix.ColumnCount, (i, j) =>
        {
            if (i < index)
            {
                return matrix[i, j];
            }
            else if (i == index)
            {
                return newRow[j];
            }
            else
            {
                return matrix[i - 1, j];
            }
        });

        return newMatrix;
    }

    Matrix<double> TransformToDenseMatrix(Matrix4x4 transformMatrix)
    {
        var denseMatrix = DenseMatrix.OfArray(new double[,]
        {
            { transformMatrix.m00, transformMatrix.m01, transformMatrix.m02, transformMatrix.m03 },
            { transformMatrix.m10, transformMatrix.m11, transformMatrix.m12, transformMatrix.m13 },
            { transformMatrix.m20, transformMatrix.m21, transformMatrix.m22, transformMatrix.m23 },
            { transformMatrix.m30, transformMatrix.m31, transformMatrix.m32, transformMatrix.m33 }
        });

        return denseMatrix;
    }

    public void solve()
    {
        get_transform();
        int from_len = from.Count;
        int to_len = to.Count;

        if (from_len == to_len)
        {
            if (from_len > 2)
            {
                SVD_solve();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
