import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { AddGroupAdminModel, GroupAdminModel, IRouteParams } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { Typeahead } from "react-bootstrap-typeahead";
import { DataTable } from "../../Shared/DataTable";

export const GroupAdmins = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each account
  const [notification, setNotification] = usePromiseNotification();
  const [groupAdmins, setGroupAdmins] = useState<GroupAdminModel[]>();
  const [adminRemoving, setAdminRemoving] = useState<number>();
  const [request, setRequest] = useState<AddGroupAdminModel>({
    lookup: "",
    group: "",
  });
  const [groups, setGroups] = useState<string[]>([]);
  const [groupSelection, setGroupSelection] = useState<string[]>([]);
  const { cluster } = useParams<IRouteParams>();

  const [getConfirmation] = useConfirmationDialog<string>(
    {
      title: "Remove Group Admin",
      message: "Are you sure you want to remove this group admin?",
    },
    []
  );

  useEffect(() => {
    const fetchGroupAdmins = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/admin/GroupAdmins`
      );

      if (response.ok) {
        setGroupAdmins(await response.json());
      } else {
        alert("Error fetching group admins");
      }
    };

    fetchGroupAdmins();
  }, [cluster]);

  useEffect(() => {
    const fetchGroups = async () => {
      const response = await authenticatedFetch(`/api/${cluster}/admin/groups`);

      if (response.ok) {
        setGroups(await response.json());
      } else {
        alert("Error fetching groups");
      }
    };

    fetchGroups();
  }, [cluster]);

  const handleRemove = async (groupUser: GroupAdminModel) => {
    const [confirmed] = await getConfirmation();
    if (!confirmed) {
      return;
    }

    setAdminRemoving(groupUser.permissionId);

    const req = authenticatedFetch(
      `/api/${cluster}/admin/RemoveGroupAdmin/${groupUser.permissionId}`,
      {
        method: "POST",
      }
    );

    setNotification(req, "Removing", "Group Admin Removed");

    const response = await req;
    if (response.ok) {
      setAdminRemoving(undefined);

      // remove the user from the list
      setGroupAdmins(
        groupAdmins?.filter((ga) => ga.permissionId !== groupUser.permissionId)
      );
    }
    //todo deal with error
  };

  const handleSubmit = async () => {
    const req = authenticatedFetch(`/api/${cluster}/admin/AddGroupAdmin`, {
      method: "POST",
      body: JSON.stringify(request),
    });

    setNotification(
      req,
      "Saving",
      (r) => "Group admin added",
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
      const newGroupUser = (await response.json()) as GroupAdminModel;
      let updatedGroupUsers = [] as GroupAdminModel[];
      //check if the newAccount is already in the list
      if (
        groupAdmins?.find((ga) => ga.permissionId === newGroupUser.permissionId)
      ) {
        //if it is, update the account
        updatedGroupUsers = groupAdmins
          ? groupAdmins.map((ga) =>
              ga.permissionId === newGroupUser.permissionId ? newGroupUser : ga
            )
          : [newGroupUser];
      } else {
        //if it is not, add it to the list and sort it
        updatedGroupUsers = groupAdmins
          ? [...groupAdmins, newGroupUser]
          : [newGroupUser];
      }
      //sort the list
      setGroupAdmins(
        updatedGroupUsers.sort(
          (a, b) =>
            (a.group ?? "").localeCompare(b.group ?? "") ||
            a.user.name.localeCompare(b.user.name)
        )
      );
      setRequest((r) => ({ ...r, lookup: "", group: "" }));
    }
  };

  if (groupAdmins === undefined) {
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
            <label className="form-label">Email or Kerberos</label>

            <input
              className="form-control"
              id="emailLookup"
              placeholder="example@ucdavis.edu"
              value={request.lookup}
              onChange={(e) =>
                setRequest((r) => ({ ...r, lookup: e.target.value }))
              }
            ></input>
          </div>
          <br />
          <div className="form-group">
            <label className="form-label">Group</label>
            <Typeahead
              id="groupTypeahead"
              options={groups}
              selected={groupSelection}
              placeholder="Select a group"
              onChange={(selected) => {
                setGroupSelection(selected.map((s) => s as string));
                if (selected.length > 0)
                  setRequest((r) => ({ ...r, group: selected[0] as string }));
                else setRequest((r) => ({ ...r, group: "" }));
              }}
            />
          </div>
          <br />
          <button
            disabled={notification.pending}
            className="btn btn-primary"
            onClick={handleSubmit}
          >
            Add Group Admin
          </button>
          <hr />

          <p>There are {groupAdmins.length} group admins</p>
          <DataTable
            keyField="id"
            data={groupAdmins}
            responsive
            columns={[
              {
                name: <b>Group</b>,
                selector: (ga) => ga.group,
                sortable: true,
              },
              {
                name: <b>User</b>,
                selector: (ga) => ga.user.name,
                sortable: true,
              },
              {
                name: <b>Email</b>,
                selector: (ga) => ga.user.email,
                sortable: true,
              },
              {
                name: <b>Action</b>,
                sortable: false,
                cell: (ga) => (
                  <>
                    <button
                      disabled={notification.pending}
                      onClick={() => handleRemove(ga)}
                      className="btn btn-danger"
                    >
                      {adminRemoving === ga.permissionId
                        ? "Removing..."
                        : "Remove"}
                    </button>
                  </>
                ),
              },
            ]}
          />
        </div>
      </div>
    );
  }
};
