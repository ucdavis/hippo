import { useCallback, useContext, useEffect, useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import AppContext from "../../Shared/AppContext";
import { GroupModel, IRouteParams } from "../../types";
import { GroupInfo } from "../Group/GroupInfo";
import { GroupLookup } from "../Group/GroupLookup";
import { CardColumns } from "reactstrap";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { usePromiseNotification } from "../../util/Notifications";
import { authenticatedFetch } from "../../util/api";
import { notEmptyOrFalsey } from "../../util/ValueChecks";

export const AccountInfo = () => {
  const [notification, setNotification] = usePromiseNotification();
  const [context] = useContext(AppContext);
  const { cluster } = useParams<IRouteParams>();
  const account = context.accounts.find((a) => a.cluster === cluster);

  const currentGroups = useMemo(() => account?.groups ?? [], [account]);

  const [groups, setGroups] = useState<GroupModel[]>([]);
  useEffect(() => {
    const fetchGroups = async () => {
      const response = await authenticatedFetch(`/api/${cluster}/group/groups`);

      if (response.ok) {
        setGroups(
          ((await response.json()) as GroupModel[]).filter(
            (g) => !currentGroups.some((cg) => cg.id === g.id)
          )
        );
      } else {
        alert("Error fetching groups");
      }
    };

    fetchGroups();
  }, [cluster, currentGroups]);

  const [getGroupConfirmation] = useConfirmationDialog<GroupModel>(
    {
      title: "Request Access to Group",
      message: (setReturn) => {
        return (
          <div className="row justify-content-center">
            <div className="col-md-8">
              <div className="form-group">
                <GroupLookup
                  setSelection={(selection) => setReturn(selection)}
                  options={groups}
                />
              </div>
            </div>
          </div>
        );
      },
      canConfirm: (returnValue) => returnValue !== undefined,
    },
    [groups]
  );

  const [getSshKeyConfirmation] = useConfirmationDialog<string>(
    {
      title: "Update your SSH Key",
      message: (setReturn) => {
        return (
          <div className="row justify-content-center">
            <div className="col-md-8">
              <div className="form-group">
                <label className="form-label">
                  What is your Public SSH key
                </label>
                <textarea
                  className="form-control"
                  id="sharedKey"
                  placeholder="Paste your public SSH key here"
                  required
                  onChange={(e) => setReturn(e.target.value)}
                ></textarea>
                <p className="form-helper">
                  Paste all of the text from your public SSH file here. Example:
                  <br></br>
                  <code>
                    ssh-rsa AAAAB3NzaC1yc....NrRFi9wrf+M7Q== fake@addr.local
                  </code>
                </p>
              </div>
            </div>
          </div>
        );
      },
      canConfirm: (returnValue) => notEmptyOrFalsey(returnValue),
    },
    []
  );

  const handleRequestAccess = useCallback(async () => {
    const [confirmed, group] = await getGroupConfirmation();
    if (confirmed) {
      const request = fetch(`/api/${cluster}/group/request/${group.id}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
      });

      setNotification(request, "Request sent", async (r) => {
        if (r.status === 400) {
          const errorText = await response.text(); //Bad Request Text
          return errorText;
        } else {
          return "An error happened, please try again.";
        }
      });

      const response = await request;
    }
  }, [cluster, getGroupConfirmation, setNotification]);

  const handleUpdateSshKey = useCallback(async () => {
    const [confirmed, sshKey] = await getSshKeyConfirmation();

    if (confirmed) {
      const request = authenticatedFetch(`/api/${cluster}/account/updatessh`, {
        method: "POST",
        body: JSON.stringify({
          accountId: account?.id,
          sshKey: sshKey,
        }),
      });

      setNotification(request, "Request sent", async (r) => {
        if (r.status === 400) {
          const errorText = await response.text(); //Bad Request Text
          return errorText;
        } else {
          return "An error happened, please try again.";
        }
      });

      const response = await request;
    }
  }, [account?.id, cluster, getSshKeyConfirmation, setNotification]);

  return (
    <div className="row justify-content-center">
      <div className="col-md-8 text-center">
        <p>
          Welcome {context.user.detail.firstName}. Your account is registered
          with the following group(s):
        </p>
        <CardColumns>
          {currentGroups.map((g, i) => (
            <GroupInfo group={g} key={i} />
          ))}
        </CardColumns>
        <br />
        <p>
          <button
            disabled={notification.pending || groups.length === 0}
            onClick={() => handleRequestAccess()}
            className="btn btn-primary"
          >
            Request Access to Another Group
          </button>
          <button
            disabled={notification.pending || groups.length === 0}
            onClick={() => handleUpdateSshKey()}
            className="btn btn-primary"
          >
            Update SSH Key
          </button>
        </p>
        <p>
          Documentation about this cluster can be found{" "}
          <a
            href="https://wiki.cse.ucdavis.edu/support/systems/farm"
            target={"_blank"}
            rel="noreferrer"
          >
            at the UC Davis Farm Wiki
          </a>
          .
        </p>
      </div>
    </div>
  );
};
