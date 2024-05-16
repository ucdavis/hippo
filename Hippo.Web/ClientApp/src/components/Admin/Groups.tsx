import { useEffect, useState, useMemo, useCallback } from "react";
import { useParams } from "react-router-dom";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { GroupModel, RawGroupModel } from "../../types";
import {
  authenticatedFetch,
  parseBadRequest,
  parseRawGroupModel,
} from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { ReactTable } from "../../Shared/ReactTable";
import { Column } from "react-table";
import { GroupNameWithTooltip } from "../Group/GroupNameWithTooltip";
import { getGroupModelString } from "../../util/StringHelpers";
import ObjectTree from "../../Shared/ObjectTree";
import GroupDetails from "../Group/GroupDetails";

export const Groups = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each account
  const [notification, setNotification] = usePromiseNotification();
  const [groups, setGroups] = useState<GroupModel[]>();
  const [editing, setEditing] = useState<number>();
  const [viewing, setViewing] = useState<number>();
  const [editGroupDisplayName, setEditGroupDisplayName] = useState<string>("");
  const { cluster } = useParams();

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
              <div className="form-group">
                <label className="form-label">Details</label>
                <ObjectTree obj={groups.find((g) => g.id === editing).data} />
              </div>
            </div>
          </div>
        );
      },
    },
    [editGroupDisplayName, setEditGroupDisplayName],
  );

  const [showDetails] = useConfirmationDialog(
    {
      title: "View Group Details",
      message: () => {
        const viewingIndex = groups.findIndex((g) => g.id === viewing);
        return <GroupDetails group={groups[viewingIndex]} />;
      },
      buttons: ["OK"],
    },
    [groups, viewing],
  );

  useEffect(() => {
    const fetchGroups = async () => {
      const response = await authenticatedFetch(`/api/${cluster}/group/groups`);

      if (response.ok) {
        setGroups(
          ((await response.json()) as RawGroupModel[]).map(parseRawGroupModel),
        );
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
        body: JSON.stringify({ id: group.id, displayName }),
      });

      setNotification(req, "Saving", "Group Updated", async (r) => {
        if (r.status === 400) {
          const errors = await parseBadRequest(response);
          return errors;
        } else {
          return "An error happened, please try again.";
        }
      });

      const response = await req;

      if (response.ok) {
        const updatedGroup = parseRawGroupModel(
          (await response.json()) as RawGroupModel,
        );
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
            (a.displayName ?? "").localeCompare(b.displayName ?? ""),
          ),
        );
        setEditGroupDisplayName("");
      }

      setEditing(undefined);
    },
    [cluster, getEditConfirmation, groups, setNotification],
  );

  const handleDetails = useCallback(
    async (group: GroupModel) => {
      setViewing(group.id);
      await showDetails();
      setViewing(undefined);
    },
    [showDetails],
  );

  const columns: Column<GroupModel>[] = useMemo(
    () => [
      {
        Header: "Group",
        accessor: (row) => getGroupModelString(row),
        Cell: (props) => (
          <GroupNameWithTooltip
            group={props.row.original}
            showDisplayName={false}
          />
        ),
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
            </button>{" "}
            <button
              disabled={notification.pending}
              onClick={() => handleDetails(props.row.original)}
              className="btn btn-primary"
            >
              {viewing === props.row.original.id ? "Viewing..." : "Details"}
            </button>
          </>
        ),
      },
    ],
    [editing, handleDetails, handleEdit, notification.pending, viewing],
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
              sortBy: [{ id: "Group" }],
            }}
          />
        </div>
      </div>
    );
  }
};
