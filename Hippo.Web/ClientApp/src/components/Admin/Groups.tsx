import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { GroupModel, IRouteParams } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { Typeahead } from "react-bootstrap-typeahead";

export const Groups = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each account
  const [notification, setNotification] = usePromiseNotification();
  const [groups, setGroups] = useState<GroupModel[]>();
  const [removing, setRemoving] = useState<number>();
  const [editing, setEditing] = useState<number>();
  const [editGroupDisplayName, setEditGroupDisplayName] = useState<string>("");
  const [request, setRequest] = useState<GroupModel>({
    id: 0,
    name: "",
    displayName: "",
  });
  const [untrackedGroupSelection, setUntrackedGroupSelection] = useState<
    string[]
  >([]);
  const [untrackedGroups, setUntrackedGroups] = useState<string[]>([]);
  const { cluster } = useParams<IRouteParams>();

  const [getRemoveConfirmation] = useConfirmationDialog<string>(
    {
      title: "Remove Group",
      message:
        "Are you sure you want to remove this group? All associated accounts will be deactivated.",
    },
    []
  );

  const [getEditConfirmation] = useConfirmationDialog<string>(
    {
      title: "Edit Group Display Name",
      message: (setReturn) => {
        return (
          <div className="row justify-content-center">
            <div className="col-md-8">
              <div className="form-group">
                <label className="form-label">Display Name</label>

                <input
                  className="form-control"
                  id="displayNameLookup"
                  placeholder="Group name or description here"
                  value={editGroupDisplayName}
                  onChange={(e) => {
                    setEditGroupDisplayName(e.target.value);
                    setReturn(e.target.value);
                  }}
                ></input>
              </div>
            </div>
          </div>
        );
      },
    },
    [editGroupDisplayName, setEditGroupDisplayName]
  );

  useEffect(() => {
    const fetchGroups = async () => {
      const response = await authenticatedFetch(`/api/${cluster}/group/groups`);

      if (response.ok) {
        setGroups(await response.json());
      } else {
        alert("Error fetching groups");
      }
    };

    fetchGroups();
  }, [cluster]);

  useEffect(() => {
    const fetchUntrackedGroups = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/group/untrackedgroups`
      );

      if (response.ok) {
        setUntrackedGroups(await response.json());
      } else {
        alert("Error fetching untracked groups");
      }
    };

    fetchUntrackedGroups();
  }, [cluster]);

  const handleRemove = async (group: GroupModel) => {
    const [confirmed] = await getRemoveConfirmation();
    if (!confirmed) {
      return;
    }

    setRemoving(group.id);

    const req = authenticatedFetch(`/api/${cluster}/group/delete/${group.id}`, {
      method: "POST",
    });

    setNotification(req, "Removing", "Group Removed");

    const response = await req;
    if (response.ok) {
      setRemoving(undefined);

      // remove the user from the list
      setGroups(groups?.filter((g) => g.id !== group.id));
    }
    //todo deal with error
  };

  const handleCreate = async () => {
    const req = authenticatedFetch(`/api/${cluster}/group/create`, {
      method: "POST",
      body: JSON.stringify(request),
    });

    setNotification(
      req,
      "Saving",
      (r) => "Group added",
      async (r) => {
        if (r.status === 400) {
          const errorText = await response.text(); //Bad Request Text
          return errorText;
        } else {
          return "An error happened, please try again.";
        }
      }
    );

    const response = await req;

    if (response.ok) {
      const newGroup = (await response.json()) as GroupModel;
      let updatedGroups = [] as GroupModel[];
      //check if the newGroup is already in the list
      if (groups?.find((g) => g.id === newGroup.id)) {
        //if it is, update the group
        updatedGroups = groups
          ? groups.map((g) => (g.id === newGroup.id ? newGroup : g))
          : [newGroup];
      } else {
        //if it is not, add it to the list and sort it
        updatedGroups = groups ? [...groups, newGroup] : [newGroup];
      }
      //sort the list
      setGroups(
        updatedGroups.sort((a, b) =>
          (a.displayName ?? "").localeCompare(b.displayName ?? "")
        )
      );
      setRequest((r) => ({ ...r, lookup: "", group: "" }));
    }
  };

  const handleEdit = async (group: GroupModel) => {
    setEditGroupDisplayName(group.displayName);
    setEditing(group.id);
    const [confirmed, displayName] = await getEditConfirmation();
    if (!confirmed) {
      setEditing(undefined);
      return;
    }

    const req = authenticatedFetch(`/api/${cluster}/group/update`, {
      method: "POST",
      body: JSON.stringify({ ...group, displayName }),
    });

    setNotification(req, "Saving", "Group Updated", async (r) => {
      if (r.status === 400) {
        const errorText = await response.text(); //Bad Request Text
        return errorText;
      } else {
        return "An error happened, please try again.";
      }
    });

    const response = await req;

    if (response.ok) {
      const updatedGroup = (await response.json()) as GroupModel;
      let updatedGroups = [] as GroupModel[];
      //check if the updatedGroup is already in the list
      if (groups?.find((g) => g.id === updatedGroup.id)) {
        //if it is, update the group
        updatedGroups = groups
          ? groups.map((g) => (g.id === updatedGroup.id ? updatedGroup : g))
          : [updatedGroup];
      } else {
        //if it is not, add it to the list and sort it
        updatedGroups = groups ? [...groups, updatedGroup] : [updatedGroup];
      }
      //sort the list
      setGroups(
        updatedGroups.sort((a, b) =>
          (a.displayName ?? "").localeCompare(b.displayName ?? "")
        )
      );
      setEditGroupDisplayName("");
    }

    setEditing(undefined);
  };

  if (groups === undefined) {
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">Loading...</div>
      </div>
    );
  } else {
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">
          <div className="form-group">
            <label className="form-label">Display Name</label>

            <input
              className="form-control"
              id="displayNameLookup"
              placeholder="Group name or description here"
              value={request.displayName}
              onChange={(e) =>
                setRequest((r) => ({ ...r, displayName: e.target.value }))
              }
            ></input>
          </div>
          <br />
          <div className="form-group">
            <label className="form-label">Group</label>
            <Typeahead
              id="groupTypeahead"
              options={untrackedGroups}
              selected={untrackedGroupSelection}
              placeholder="Select a group"
              onChange={(selected) => {
                setUntrackedGroupSelection(selected.map((s) => s as string));
                if (selected.length > 0)
                  setRequest((r) => ({ ...r, name: selected[0] as string }));
                else setRequest((r) => ({ ...r, name: "" }));
              }}
            />
          </div>
          <br />
          <button
            disabled={
              notification.pending || !request.name || !request.displayName
            }
            className="btn btn-primary"
            onClick={handleCreate}
          >
            Add Group
          </button>
          <hr />

          <p>There are {groups.length} groups</p>
          <table className="table">
            <thead>
              <tr>
                <th>Group</th>
                <th>Display Name</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {groups.map((g) => (
                <tr key={g.id}>
                  <td>{g.name}</td>
                  <td>{g.displayName}</td>
                  <td>
                    <button
                      disabled={notification.pending}
                      onClick={() => handleRemove(g)}
                      className="btn btn-danger"
                    >
                      {removing === g.id ? "Removing..." : "Remove"}
                    </button>
                    |
                    <button
                      disabled={notification.pending}
                      onClick={() => handleEdit(g)}
                      className="btn btn-primary"
                    >
                      {editing === g.id ? "Updating..." : "Edit"}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    );
  }
};
