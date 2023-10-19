import { useEffect, useState, useMemo, useCallback } from "react";
import { useParams } from "react-router-dom";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { GroupModel, IRouteParams } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { ReactTable } from "../../Shared/ReactTable";
import { Column } from "react-table";

export const Groups = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each account
  const [notification, setNotification] = usePromiseNotification();
  const [groups, setGroups] = useState<GroupModel[]>();
  const [editing, setEditing] = useState<number>();
  const [editGroupDisplayName, setEditGroupDisplayName] = useState<string>("");
  const { cluster } = useParams<IRouteParams>();

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
      const response = await authenticatedFetch(
        `/api/${cluster}/group/groupnames`
      );

      if (response.ok) {
        setGroups(await response.json());
      } else {
        alert("Error fetching groups");
      }
    };

    fetchGroups();
  }, [cluster]);

  const handleEdit = useCallback(
    async (group: GroupModel) => {
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
    },
    [cluster, getEditConfirmation, groups, setNotification]
  );

  const columns: Column<GroupModel>[] = useMemo(
    () => [
      {
        Header: "Group",
        accessor: (group) => group.name,
      },
      {
        Header: "Display Name",
        accessor: (group) => group.displayName,
      },
      {
        Header: "Action",
        Cell: (props) => (
          <>
            <button
              disabled={notification.pending}
              onClick={() => handleEdit(props.row.original)}
              className="btn btn-primary"
            >
              {editing === props.row.original.id ? "Updating..." : "Edit"}
            </button>
          </>
        ),
      },
    ],
    [editing, handleEdit, notification.pending]
  );

  const groupsData = useMemo(() => groups ?? [], [groups]);

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
          <p>There are {groups.length} groups</p>
          <ReactTable
            columns={columns}
            data={groupsData}
            initialState={{
              sortBy: [{ id: "name" }],
            }}
          />
        </div>
      </div>
    );
  }
};
