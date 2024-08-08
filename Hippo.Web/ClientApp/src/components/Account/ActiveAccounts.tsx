import { useEffect, useState, useMemo, useCallback } from "react";
import { useParams } from "react-router-dom";
import { AccountModel, AccountTagsModel } from "../../types";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import { ReactTable } from "../../Shared/ReactTable";
import { createColumnHelper } from "@tanstack/react-table";
import { GroupNameWithTooltip } from "../Group/GroupNameWithTooltip";
import { getGroupModelString } from "../../util/StringHelpers";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import SearchTags from "../../Shared/SearchTags";
import ObjectTree from "../../Shared/ObjectTree";
import { usePromiseNotification } from "../../util/Notifications";

export const ActiveAccounts = () => {
  const [notification, setNotification] = usePromiseNotification();
  const [accounts, setAccounts] = useState<AccountModel[]>();
  const [viewing, setViewing] = useState<AccountModel>();
  const [editing, setEditing] = useState<AccountModel>();

  const { cluster } = useParams();
  const [tags, setTags] = useState<string[]>([]);
  useEffect(() => {
    const fetchTags = async () => {
      const response = await authenticatedFetch(`/api/${cluster}/tags/getall`);
      const data = await response.json();
      setTags(data);
    };

    fetchTags();
  }, [cluster]);

  const [showDetails] = useConfirmationDialog(
    {
      title: "Account Details",
      message: () => {
        const account = accounts.find((a) => a.id === viewing.id);
        return (
          <div className="row justify-content-center">
            <div className="col-md-12">
              <div className="form-group">
                <label className="form-label">Name</label>
                <input
                  className="form-control"
                  id="accountDetailsName"
                  value={account.name}
                  readOnly
                ></input>
              </div>
              <div className="form-group">
                <label className="form-label">Email</label>
                <input
                  className="form-control"
                  id="accountDetailsEmail"
                  value={account.email}
                  readOnly
                ></input>
              </div>
              <div className="form-group">
                <label className="form-label">Kerberos</label>
                <input
                  className="form-control"
                  id="accountDetailsKerberos"
                  value={account.kerberos}
                  readOnly
                ></input>
              </div>
              <div className="form-group">
                <label className="form-label">Updated On</label>
                <input
                  className="form-control"
                  id="accountDetailsUpdatedOn"
                  value={new Date(account.updatedOn).toLocaleDateString()}
                  readOnly
                ></input>
              </div>
              <div className="form-group">
                <label className="form-label">Tags</label>
                <SearchTags
                  onSelect={setTags}
                  disabled={true}
                  selected={account.tags}
                  options={[]}
                  placeHolder={""}
                  id={"accountTags"}
                />
              </div>
              <div className="form-group">
                <label className="form-label">Details</label>
                <ObjectTree obj={account.data} />
              </div>
            </div>
          </div>
        );
      },
      buttons: ["OK"],
    },
    [accounts, viewing],
  );

  const [editTags] = useConfirmationDialog<string[]>(
    {
      title: "Edit Account Tags",
      message: (setReturn) => {
        return (
          <SearchTags
            onSelect={(tags) => {
              setReturn(tags);
              setEditing((editing) => ({ ...editing, tags }));
            }}
            disabled={false}
            options={tags}
            selected={editing.tags}
            placeHolder={"Enter tags here"}
            id={"searchTags"}
          />
        );
      },
    },
    [accounts, editing],
  );

  const handleDetails = useCallback(
    async (account: AccountModel) => {
      setViewing({ ...account });
      await showDetails();
      setViewing(undefined);
    },
    [showDetails],
  );

  const handleEditTags = async (account: AccountModel) => {
    setEditing({ ...account });
    const [confirmed, newTags] = await editTags();
    if (confirmed) {
      const accountTagsModel: AccountTagsModel = {
        AccountId: account.id,
        Tags: newTags,
      };
      const request = authenticatedFetch(
        `/api/${cluster}/tags/UpdateAccountTags`,
        {
          method: "POST",
          body: JSON.stringify(accountTagsModel),
        },
      );
      setNotification(request, "Updating Tags", "Tags Updated", async (r) => {
        if (r.status === 400) {
          const errors = await parseBadRequest(response);
          return errors;
        } else {
          return "An error happened, please try again.";
        }
      });

      const response = await request;
      if (response.ok) {
        setAccounts((accounts) => [
          ...accounts.filter((a) => a.id !== account.id),
          { ...account, tags: newTags },
        ]);
      }
    }
    setEditing(undefined);
  };

  const columnHelper = createColumnHelper<AccountModel>();

  const columns = [
    columnHelper.accessor((row) => getGroupModelString(row.memberOfGroups), {
      header: "Groups",
      cell: (props) => (
        <>
          {props.row.original.memberOfGroups.map((g, i) => (
            <>
              {i > 0 && ", "}
              <GroupNameWithTooltip
                group={g}
                id={props.row.original.id.toString()}
                key={i}
              />
            </>
          ))}
        </>
      ),
      meta: {
        exportFn: (account) =>
          account.memberOfGroups.map((g) => g.displayName).join(", "),
      },
    }),
    columnHelper.accessor("name", {
      header: "Name",
      id: "Name", // id required for only this column for some reason
    }),
    columnHelper.accessor("email", {
      header: "Email",
    }),
    columnHelper.accessor("kerberos", {
      header: "Kerberos",
    }),
    columnHelper.accessor(
      (row) => new Date(row.updatedOn).toLocaleDateString(),
      {
        header: "Updated On",
      },
    ),
    columnHelper.accessor((row) => row.tags.join(", "), {
      header: "Tags",
    }),
    columnHelper.display({
      id: "actions",
      header: "Action",
      cell: (props) => (
        <>
          <button
            onClick={() => handleDetails(props.row.original)}
            className="btn btn-primary"
          >
            {viewing?.id === props.row.original.id ? "Viewing..." : "Details"}
          </button>{" "}
          |{" "}
          <button
            onClick={() => handleEditTags(props.row.original)}
            className="btn btn-primary"
          >
            {editing?.id === props.row.original.id ? "Editing..." : "Edit Tags"}
          </button>{" "}
        </>
      ),
    }),
  ];

  const accountsData = useMemo(() => accounts ?? [], [accounts]);

  useEffect(() => {
    const fetchAccounts = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/account/active`,
      );

      if (response.ok) {
        setAccounts((await response.json()) as AccountModel[]);
      }
    };

    fetchAccounts();
  }, [cluster]);

  if (accounts === undefined) {
    return (
      <div className="row justify-content-center">
        <div className="col-md-12">Loading...</div>
      </div>
    );
  } else {
    const groupCount = new Set(
      accounts
        .map((a) => a.memberOfGroups)
        .flat()
        .filter((g) => g !== null)
        .map((g) => g.name),
    ).size;
    return (
      <div className="row justify-content-center">
        <div className="col-md-12">
          <p>
            You have {accounts.length} active account(s) in {groupCount}{" "}
            group(s)
          </p>
          <ReactTable
            columns={columns}
            data={accountsData}
            initialState={{
              sorting: [
                { id: "Groups", desc: false },
                { id: "Name", desc: false },
              ],
            }}
          />
        </div>
      </div>
    );
  }
};
