import { useEffect, useState, useMemo, useCallback } from "react";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { ClusterModel, Cluster } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { notEmptyOrFalsey } from "../../util/ValueChecks";
import { ReactTable } from "../../Shared/ReactTable";
import { Column } from "react-table";

const defaultCluster: Cluster = {
  id: 0,
  name: "",
  description: "",
  sshName: "",
  sshKeyId: "",
  sshUrl: "",
  domain: "",
};

export const Clusters = () => {
  const [notification, setNotification] = usePromiseNotification();
  const [clusterModels, setClusters] = useState<ClusterModel[]>([]);
  const [editClusterModel, setEditClusterModel] = useState<ClusterModel>({
    cluster: { ...defaultCluster },
  });
  const [editConfirmationTitle, setEditConfirmationTitle] = useState("");

  const [getEditConfirmation] = useConfirmationDialog<ClusterModel>(
    {
      title: editConfirmationTitle,
      message: (setReturn) => (
        <>
          <div className="form-group">
            <label htmlFor="fieldName">Name</label>
            <input
              className="form-control"
              id="fieldName"
              required
              value={editClusterModel.cluster.name}
              onChange={(e) => {
                const model: ClusterModel = {
                  ...editClusterModel,
                  cluster: {
                    ...editClusterModel.cluster,
                    name: e.target.value,
                  },
                };
                setEditClusterModel(model);
                setReturn(model);
              }}
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldDescription">Description</label>
            <input
              className="form-control"
              id="fieldDescription"
              required
              value={editClusterModel.cluster.description}
              onChange={(e) => {
                const model: ClusterModel = {
                  ...editClusterModel,
                  cluster: {
                    ...editClusterModel.cluster,
                    description: e.target.value,
                  },
                };
                setEditClusterModel(model);
                setReturn(model);
              }}
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldSshUrl">SSH Url</label>
            <input
              className="form-control"
              id="fieldSshUrl"
              value={editClusterModel.cluster.sshUrl}
              onChange={(e) => {
                const model: ClusterModel = {
                  ...editClusterModel,
                  cluster: {
                    ...editClusterModel.cluster,
                    sshUrl: e.target.value,
                  },
                };
                setEditClusterModel(model);
                setReturn(model);
              }}
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldSshName">SSH Name</label>
            <input
              className="form-control"
              id="fieldSshName"
              value={editClusterModel.cluster.sshName}
              onChange={(e) => {
                const model: ClusterModel = {
                  ...editClusterModel,
                  cluster: {
                    ...editClusterModel.cluster,
                    sshName: e.target.value,
                  },
                };
                setEditClusterModel(model);
                setReturn(model);
              }}
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldSshKey">SSH Key</label>
            <textarea
              className="form-control"
              id="fieldSshKey"
              rows={3}
              wrap="soft"
              value={editClusterModel.sshKey}
              onChange={(e) => {
                const model: ClusterModel = {
                  ...editClusterModel,
                  cluster: { ...editClusterModel.cluster },
                  sshKey: e.target.value,
                };
                setEditClusterModel(model);
                setReturn(model);
              }}
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldDomain">Domain</label>
            <input
              className="form-control"
              id="fieldDomain"
              value={editClusterModel.cluster.domain}
              onChange={(e) => {
                const model: ClusterModel = {
                  ...editClusterModel,
                  cluster: {
                    ...editClusterModel.cluster,
                    domain: e.target.value,
                  },
                };
                setEditClusterModel(model);
                setReturn(model);
              }}
            />
          </div>
        </>
      ),
      canConfirm:
        notEmptyOrFalsey(editClusterModel.cluster.name) &&
        notEmptyOrFalsey(editClusterModel.cluster.description) &&
        !notification.pending,
    },
    [
      editClusterModel,
      setEditClusterModel,
      notification.pending,
      editConfirmationTitle,
    ]
  );

  const [getRemoveConfirmation] = useConfirmationDialog<string>(
    {
      title: "Remove Cluster",
      message: "Are you sure you want to remove this cluster?",
    },
    []
  );

  useEffect(() => {
    const fetchClusters = async () => {
      const response = await authenticatedFetch(`/api/clusteradmin/clusters`);

      if (response.ok) {
        setClusters(await response.json());
      } else {
        alert("Error");
      }
    };

    fetchClusters();
  }, []);

  const handleRemove = useCallback(
    async (id: number) => {
      const [confirmed] = await getRemoveConfirmation();
      if (!confirmed) {
        return;
      }

      const req = authenticatedFetch(`/api/clusteradmin/delete/${id}`, {
        method: "POST",
      });

      setNotification(req, "Removing", "Cluster Removed");

      const response = await req;
      if (response.ok) {
        // remove the cluster from the list
        setClusters(clusterModels.filter((m) => m.cluster.id !== id));
      }
      //todo deal with error
    },
    [clusterModels, getRemoveConfirmation, setNotification]
  );

  const handleCreate = async () => {
    setEditClusterModel({ cluster: { ...defaultCluster }, sshKey: "" });
    setEditConfirmationTitle("Create Cluster");
    const [confirmed, newModel] = await getEditConfirmation();

    if (!confirmed) {
      return;
    }

    const req = authenticatedFetch(`/api/clusteradmin/create`, {
      method: "POST",
      body: JSON.stringify(newModel),
    });

    setNotification(req, "Saving", "Cluster Created", async (r) => {
      if (r.status === 400) {
        const errorText = await response.text(); //Bad Request Text
        return errorText;
      } else {
        return "An error happened, please try again.";
      }
    });

    const response = await req;

    if (response.ok) {
      const newCluster = await response.json();
      setClusters(
        [...clusterModels, newCluster].sort((a, b) =>
          a.cluster.name.localeCompare(b.cluster.name)
        )
      );
      setEditClusterModel((r) => ({ ...r, lookup: "", name: "" }));
    }
  };

  const handleEdit = useCallback(
    async (id: number) => {
      const editClusterModel = clusterModels.filter(
        (m) => m.cluster.id === id
      )[0];
      setEditClusterModel({
        cluster: { ...editClusterModel.cluster },
        sshKey: editClusterModel.sshKey,
      });
      setEditConfirmationTitle("Edit Cluster");
      const [confirmed, newModel] = await getEditConfirmation();

      if (!confirmed) {
        return;
      }

      const req = authenticatedFetch(`/api/clusteradmin/update`, {
        method: "POST",
        body: JSON.stringify(newModel),
      });

      setNotification(req, "Saving", "Cluster Updated", async (r) => {
        if (r.status === 400) {
          const errorText = await response.text(); //Bad Request Text
          return errorText;
        } else {
          return "An error happened, please try again.";
        }
      });

      const response = await req;

      if (response.ok) {
        const newCluster = await response.json();
        setClusters(
          clusterModels
            .map((m) => (m.cluster.id === id ? newCluster : m))
            .sort((a, b) => a.cluster.name.localeCompare(b.cluster.name))
        );
        setEditClusterModel((r) => ({ ...r, lookup: "", name: "" }));
      }
    },
    [clusterModels, getEditConfirmation, setNotification]
  );

  const columns: Column<ClusterModel>[] = useMemo(
    () => [
      {
        Header: "Name",
        accessor: (m) => m.cluster.name,
        sortable: true,
        wrap: true,
        width: "100px",
      },
      {
        Header: "Description",
        accessor: (m) => m.cluster.description,
        sortable: true,
        wrap: true,
        width: "100px",
      },
      {
        Header: "SSH URL",
        accessor: (m) => m.cluster.sshUrl,
        sortable: true,
        wrap: true,
      },
      {
        Header: "SSH Name",
        accessor: (m) => m.cluster.sshName,
        sortable: true,
        wrap: true,
        width: "100px",
      },
      {
        Header: "SSH Key ID",
        accessor: (m) => m.cluster.sshKeyId,
        sortable: true,
        wrap: true,
      },
      {
        Header: "Domain",
        accessor: (m) => m.cluster.domain,
        sortable: true,
        wrap: true,
      },
      {
        Header: "Action",
        sortable: false,
        Cell: (m) => (
          <>
            <button
              disabled={notification.pending}
              onClick={() => handleEdit(m.cluster.id)}
              className="btn btn-primary"
            >
              Edit
            </button>
            {" | "}
            <button
              disabled={notification.pending}
              onClick={() => handleRemove(m.cluster.id)}
              className="btn btn-danger"
            >
              Remove
            </button>
          </>
        ),
      },
    ],
    [handleEdit, handleRemove, notification.pending]
  );

  const clusterModelsData = useMemo(() => clusterModels ?? [], [clusterModels]);

  if (notification.pending) {
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">Loading...</div>
      </div>
    );
  } else {
    return (
      <>
        <div className="row justify-content-center">
          <div className="col-md-8">
            <div className="float-end">
              <button onClick={handleCreate} className="btn btn-primary">
                Create
              </button>
            </div>
          </div>
          <div className="col-md-8">
            <ReactTable
              columns={columns}
              data={clusterModelsData}
              initialState={{
                sortBy: [{ id: "name" }],
              }}
            />
          </div>
        </div>
      </>
    );
  }
};
