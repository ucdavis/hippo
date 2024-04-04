import { useState, useMemo, useCallback, useContext } from "react";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { ClusterModel, AccessType } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { notEmptyOrFalsey } from "../../util/ValueChecks";
import { ReactTable } from "../../Shared/ReactTable";
import { Column } from "react-table";
import SshKeyInput from "../../Shared/SshKeyInput";
import SearchDefinedOptions from "../../Shared/SearchDefinedOptions";
import { AccessTypes } from "../../constants";
import AppContext from "../../Shared/AppContext";

const defaultCluster: ClusterModel = {
  id: 0,
  name: "",
  description: "",
  sshName: "",
  sshKeyId: "",
  sshUrl: "",
  domain: "",
  email: "",
  accessTypes: AccessTypes,
};

export const Clusters = () => {
  const [notification, setNotification] = usePromiseNotification();
  const [editClusterModel, setEditClusterModel] = useState<ClusterModel>({
    ...defaultCluster,
  });
  const [editConfirmationTitle, setEditConfirmationTitle] = useState("");
  const [context, setContext] = useContext(AppContext);

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
              value={editClusterModel.name}
              onChange={(e) => {
                const model: ClusterModel = {
                  ...editClusterModel,
                  name: e.target.value,
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
              value={editClusterModel.description}
              onChange={(e) => {
                const model: ClusterModel = {
                  ...editClusterModel,
                  description: e.target.value,
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
              value={editClusterModel.sshUrl}
              onChange={(e) => {
                const model: ClusterModel = {
                  ...editClusterModel,
                  sshUrl: e.target.value,
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
              value={editClusterModel.sshName}
              onChange={(e) => {
                const model: ClusterModel = {
                  ...editClusterModel,
                  sshName: e.target.value,
                };
                setEditClusterModel(model);
                setReturn(model);
              }}
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldSshKey">SSH Key</label>
            <SshKeyInput
              onChange={(value) => {
                const model: ClusterModel = {
                  ...editClusterModel,
                  sshKey: value,
                };
                setEditClusterModel(model);
                setReturn({ ...model, sshKey: value.trim() });
              }}
            />
            <p className="form-helper">
              To generate a ssh key pair please see this link:{" "}
              <a href="https://hpc.ucdavis.edu/faq#ssh-key" target={"blank"}>
                https://hpc.ucdavis.edu/faq#ssh-key
              </a>
            </p>
          </div>
          <div className="form-group">
            <label htmlFor="fieldDomain">Domain</label>
            <input
              className="form-control"
              id="fieldDomain"
              value={editClusterModel.domain}
              onChange={(e) => {
                const model: ClusterModel = {
                  ...editClusterModel,
                  domain: e.target.value,
                };
                setEditClusterModel(model);
                setReturn(model);
              }}
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldEmail">Email</label>
            <input
              className="form-control"
              id="fieldEmail"
              value={editClusterModel.email}
              onChange={(e) => {
                const model: ClusterModel = {
                  ...editClusterModel,
                  email: e.target.value,
                };
                setEditClusterModel(model);
                setReturn(model);
              }}
            />
          </div>
          <div className="form-group">
            <label className="form-label">Access Type</label>
            <SearchDefinedOptions<AccessType>
              definedOptions={AccessTypes}
              selected={editClusterModel.accessTypes}
              onSelect={(accessTypes) => {
                const model: ClusterModel = {
                  ...editClusterModel,
                  accessTypes,
                };
                setEditClusterModel(model);
                setReturn(model);
              }}
              disabled={false}
              placeHolder="Select one or more access types"
              id="selectAccessTypes"
            />
          </div>
        </>
      ),
      canConfirm:
        notEmptyOrFalsey(editClusterModel.name) &&
        notEmptyOrFalsey(editClusterModel.description) &&
        !!editClusterModel.accessTypes.length &&
        !notification.pending,
    },
    [
      editClusterModel,
      setEditClusterModel,
      notification.pending,
      editConfirmationTitle,
    ],
  );

  const [getDetailsConfirmation] = useConfirmationDialog<ClusterModel>(
    {
      title: editConfirmationTitle,
      message: () => (
        <>
          <div className="form-group">
            <label htmlFor="fieldName">Name</label>
            <input
              className="form-control"
              id="fieldName"
              required
              value={editClusterModel.name}
              readOnly
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldDescription">Description</label>
            <input
              className="form-control"
              id="fieldDescription"
              required
              value={editClusterModel.description}
              readOnly
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldSshUrl">SSH Url</label>
            <input
              className="form-control"
              id="fieldSshUrl"
              value={editClusterModel.sshUrl}
              readOnly
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldSshName">SSH Name</label>
            <input
              className="form-control"
              id="fieldSshName"
              value={editClusterModel.sshName}
              readOnly
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldDomain">Domain</label>
            <input
              className="form-control"
              id="fieldDomain"
              value={editClusterModel.domain}
              readOnly
            />
          </div>
          <div className="form-group">
            <label htmlFor="fieldEmail">Email</label>
            <input
              className="form-control"
              id="fieldEmail"
              value={editClusterModel.email}
              readOnly
            />
          </div>
          <div className="form-group">
            <label className="form-label">Access Type</label>
            <SearchDefinedOptions<AccessType>
              definedOptions={AccessTypes}
              selected={editClusterModel.accessTypes}
              onSelect={() => {}}
              disabled={true}
              placeHolder="none selected"
              id="selectedAccessTypes"
            />
          </div>
        </>
      ),
      buttons: ["OK"],
    },
    [
      editClusterModel,
      setEditClusterModel,
      notification.pending,
      editConfirmationTitle,
    ],
  );

  const [getRemoveConfirmation] = useConfirmationDialog<string>(
    {
      title: "Remove Cluster",
      message: "Are you sure you want to remove this cluster?",
    },
    [],
  );

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
        setContext((c) => ({
          ...c,
          clusters: c.clusters.filter((c) => c.id !== id),
        }));
      }
      //todo deal with error
    },
    [getRemoveConfirmation, setNotification, setContext],
  );

  const handleCreate = async () => {
    setEditClusterModel({ ...defaultCluster, sshKey: "" });
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
      const newCluster = (await response.json()) as ClusterModel;
      setContext((c) => ({
        ...c,
        clusters: [...c.clusters, newCluster].sort((a, b) =>
          a.name.localeCompare(b.name),
        ),
      }));
      setEditClusterModel((r) => ({ ...r, lookup: "", name: "" }));
    }
  };

  const handleEdit = useCallback(
    async (id: number) => {
      const editClusterModel = context.clusters.filter((m) => m.id === id)[0];
      setEditClusterModel({
        ...editClusterModel,
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
        setContext((c) => ({
          ...c,
          clusters: c.clusters
            .map((m) => (m.id === id ? newCluster : m))
            .sort((a, b) => a.name.localeCompare(b.name)),
        }));
        setEditClusterModel((r) => ({ ...r, lookup: "", name: "" }));
      }
    },
    [context.clusters, getEditConfirmation, setContext, setNotification],
  );

  const handleDetails = useCallback(
    async (id: number) => {
      const editClusterModel = context.clusters.filter((m) => m.id === id)[0];
      setEditClusterModel({
        ...editClusterModel,
        sshKey: editClusterModel.sshKey,
      });
      setEditConfirmationTitle("Cluster Details");
      await getDetailsConfirmation();
    },
    [context.clusters, getDetailsConfirmation],
  );

  const columns: Column<ClusterModel>[] = useMemo(
    () => [
      {
        Header: "Name",
        accessor: (m) => m.name,
        sortable: true,
        wrap: true,
        width: "100px",
      },
      {
        Header: "Description",
        accessor: (m) => m.description,
        sortable: true,
        wrap: true,
        width: "100px",
      },
      {
        Header: "Domain",
        accessor: (m) => m.domain,
        sortable: true,
        wrap: true,
      },
      {
        Header: "Email",
        accessor: (m) => m.email,
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
              onClick={() => handleDetails(m.row.original.id)}
              className="btn btn-primary"
            >
              Details
            </button>
            {" | "}
            <button
              disabled={notification.pending}
              onClick={() => handleEdit(m.row.original.id)}
              className="btn btn-primary"
            >
              Edit
            </button>
            {" | "}
            <button
              disabled={notification.pending}
              onClick={() => handleRemove(m.row.original.id)}
              className="btn btn-danger"
            >
              Remove
            </button>
          </>
        ),
      },
    ],
    [handleDetails, handleEdit, handleRemove, notification.pending],
  );

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
            data={context.clusters}
            initialState={{
              sortBy: [{ id: "Name" }],
            }}
          />
        </div>
      </div>
    </>
  );
};
