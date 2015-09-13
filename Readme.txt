Wed, 09 May 2012  00:37
updated Fri, 01 Jun 2012  00:49


CleanModQueue is a program to automatically and continuously clean the
moderator queue on reddit, according to rules and settings you provide.

Reddit is the social news and discussion site. http://www.reddit.com .

Moderators are responsible for responding to reports, and removing or
approving posts in their moderation queue, also known as the
modqueue. In some busy reddits, the modqueue gets lots of noise traffic
- posts that are off topic, pure advertisements, or otherwise
inappropriate.  These posts are known as spam. In other reddits,
moderators may want to personally and explicitly approve each post.

This tool can help.  This tool monitors the moderation queue and applies
rules to take action for items in the queue. It can remove posts that
appear to be spam, and approve posts that appear to be good ones.

The moderator still needs to personally monitor the modqueue, but by
using this tool, much of the busy work is eliminated.

Monitoring the modqueue is the original purpose of the CleanModQueue
tool, but it has been extended to also monitor the new queue as well.
Moderators can apply a distinct set of rules to the new queue - for
example removing posts that refer to a particular domain.  This is
possible even if the posts do not appear in the mod queue.


Dependencies
======================

CleanModQueue depends on .NET 4.0, and runs on Windows.  There is no
installer for CleanModQueue at this time, therefore, you need to insure
the pre-requisites are present, yourself.  You may need to download and
install .NET 4.0:

  http://www.microsoft.com/en-us/download/details.aspx?id=17851


Usage / How it works
======================

To use it, just unpack the binary zip into a new directory.
Then run the executable.

The first time you run the tool, you will see the Settings form.  On
this form you should configure your Reddit User and Password, as well as
other settings relevant to the tool, including:

  - the deny list of authors and domains
  - the blessed list for authors and domains
  - the upvote threshold
  - the order of evaluation of rules
  - the recheck interval.
  - whether to remember an author of any spam post as a spammer

After you specify the necessary settings, the tool uses the credentials
you provide to programmatically login to reddit. It then retrieves the
contents of your modqueue and displays the results.

The program then walks through each item in the queue and applies the
processing rules to that item.  The order of evaluation of the rules is
this:

 - first, examine explicit checkboxes on any item.  These take
   precedence over any rule evaluation.  There's a checkbox for "spam",
   and one for "remove" and one for "approve."  If any of these are
   checked, then CleanModQueue does the intended thing for the given
   post.

   There is one extra step, if you have checked the "spam" box for a
   line item. In that case, the post is marked as spam, and
   then, optionally, depending on the setting you have selected in the
   configuration form, the tool will remember that the author of that post
   as a spammer. In this case, in the future any new post authored by that
   user will automatically be marked as spam. This is known as the
   "Spam is as spam does" checkbox.

   The converse is not also true. When you approve a post with the
   checkbox, the author of that post is never automatically added to the
   list of blessed authors.

 - if none of the checkboxes are ticked, the tool applies the rules to
   the item, in the order you've specified. If the domain on the post
   matches one of the blocked domains you've configured, then the post
   will be removed.


-------------------------------------------------------

For domains:

Marking a domain as "spam" or "blessed" affects all subdomains.  If you
mark fbcdn.com as blocked, then any post from a domain that equals
fbcdn.com or ends in .fbcdn.com will be blocked.

-------------------------------------------------------

Example 1:

Suppose you specify that the order should be Deny Users,
Allow Users. (We won't concern ourselves with the other types of rules,
for the purposes of this simple example.)

On the Spam tab, in the list of spammy users, you specify 4 reddit user
names. On the Blessed tab, you also check the box under "allow posts
from all users". You leave the lists for the Spam and Blessed domains
blank.

The tool will loop through the list of items on the queue.  Because you
have specified that the Deny Users rule runs first, the tool will first
examine the author of the post, and compare it against the list of 4
spammy authors. If there is a match, the post is removed (spammed).

If there is no match, then the tool proceeds to the next rule type -
Allow Users. Because you've ticked the "Allow all" box, the post is
approved.

Processing then moves to the next item in the queue.

This combination of denying specific users, and allowing all others,
allows you to use the tool in a simple blacklist mode. The ordering
of rule evaluation is obviously important - if you allow the post before
checking whether the user is on the deny list, then the tool would not
function as a blacklist.

=======================================================


Example 2:

Suppose you specify the order should be Deny Users, Deny Domains, Allow
Upvotes, Allow Users, Allow Domains.

On the Spam tab, you specify a set of users and domains in the
appropriate lists. On the Blessed tab, you specify nothing.  On both
tabs, all boxes are unchecked.

In this case,
the tool will loop through the list of items on the queue.  Because you
have specified that the Deny Users rule runs first, the tool will
examine the author of the post, and compare it against the list of 4
spammy authors. If there is a match, the post is removed (spammed).

If there is no match, the
tool will move to the next rule - Deny Domains. It compares the domain
of the post against the spam list. If there is a match, the tool removes
the post.

If there is still no match, the tool checks the upvotes on the post. If
the number of upvotes on the post exceeds the threshold, the tool
approves the post.

If the post does not have the necessary upvotes, then the post remains
on the queue, and the tool moves to the next item.


Other examples:

- to specify an author whitelist: In the Settings dialog, provide the
  list of known good authors in the textbox for blessed authors. Don't
  specify anything in the textbox for known spammer authors. Tick the
  box for "deny all authors". Specify ordering to be: Approve Users,
  Deny Users. Leave everything empty for known good and known spammy
  domains.

- to blacklist some domains: In the Settings dialog, give the list of
  known bad domains in the textbox for known spam domains. Specify
  nothing else in any other textboxes. Don't tick any checkboxes for
  wildcards. Specify ordering as: Deny Domains, and then anything else.

- to blacklist some domains and some users: In the Settings dialog,
  provide the list of known bad domains in the textbox for known spam
  domains. Likewise, provide the list of known spam authors in the
  appropriate textbox. Specify nothing else in any other
  textboxes. Don't tick any checkboxes for wildcards. Specify ordering
  as: Deny Domains, Deny Authors.

- to blacklist some things and whitelist others: Do as above, but also
  specify known good authors and domains. Don't tick any checkboxes for
  wildcards. Specify ordering as desired for your situation. Ordering
  will determine rule priority. For example, if there is a post from a
  "known spam" author referring to a "known good" domain, that
  represents a conflict and your rule priority stipulates how to resolve
  it. If the ordering has Deny Users before Allow Domains, then that
  post will be removed. If the converse, then that post will be
  approved.

-----

There are some built-in rules.

If the post is from a "shadow-banned" user - this is one that is a known
spammer and has been shadow-removed on reddit - then the tool removes
that post, always.

If you view your modqueue in a browser, shadow-banned posts appear with
their titles shown in strikethrough text. My opinion is that these posts
should never appear in any modqueue since they are from known
spammers. But I suppose some people might want to review them
individually.  In any case, CleanModQueue takes a dim view of
shadow-banned posts - just marking them as spam automatically.

Also, there's another built-in rule that checks for posts from mods of
the reddit. Those posts or comments are always automatically approved.
(It may make sense in the future to make this rule optional, but for now
it is hard-coded.)

These two built-in rules are applied at lower precedence than the other
rules that you may explicitly specify.  If you watch the tool in
operation, you'll see that it processes the list of items in each queue
twice. The second sweep is to handle these lower-priority built-in
rules.

-----


Monitor Multiple Queues

Starting in v2012.05.31, the tool can monitor multiple independent
queues.  It can monitor the modqueue for a specific reddit, and apply
one set of rules and ordering, and it can also monitor the new queue of
a different reddit, and apply a different set of rules and ordering.

There's a dropdown list (combobox) on the settings form that allows you
to select a queue. The list is populated with the reddits for which you
are a moderator.  The first option is always present - it is "modqueue"
and is the aggregated queue for all reddits that you moderate. The rest
of the options are the new and mod queues for particular reddits.

If you select one of the other options in that dropdown, be aware that
you must tick the "Enabled" checkbox in the Basic tab of the settings
panel, to tell the tool to actually use your rules at runtime.

If you moderate several reddits and need to apply different rules to
each of them, you may wish to disable monitoring of the aggregate
modqueue - untick the Enabled checkbox. Then you can specify a
particular set of rules for each reddit.

For individual reddits, you may monitor the new queue for that reddit, or the
moderation queue for that reddit, or both. Select what you want in the
dropdown box.

If you specify rules for several different reddits, when re-scanning,
the tool will apply the rules to each reddit queue in order, and will
display the results for each queue in a distinct tab in the main form.



Example 3:

Suppose you moderate one reddit; you can set up a list of known-spammer
authors and known-spam domains for the mod queue for a reddit. Then, you
set up an additional set of known-spam domains for the new queue of the
same reddit.

The effect is to remove any post caught in the modqueue that matches any
of the rules.  It will also proactively check the new queue periodically
and comb out posts that do not get flagged as potential spam by
reddit. The "known spam" domains are different for each queue, though
you can make them the same via copy/paste.

When checking the new queue, there is a race condition: The tool
retrieves only the most recent 25 items via the reddit HTTP API.  If a
spammy post enters the new queue and rolls out of the top 25 in between
scans by this tool, this tool can miss the item, and it will then fail
to delete it. This problem can be mitigated somewhat by shortening the
re-scan interval. If you tell the tool to check every 3 minutes, it
would only miss a post in the new queue if you get more than 25 posts in
180 seconds.  It's unlikely that this will occur, but in fact the race
condition remains. Be aware.


=======================================================

After the tool applies its rules to the posts in a particular queue, it
displays any posts remaining in that queue, in a grid on the form. You
can then tick the boxes to tell the tool to manually spam, remove, or
approve items that remain in the queue. The tool will apply these
actions after the re-check interval expires, or you can click the
"Process" button to do it "right now."

If you want to refresh the list of items, click the "Check" button.
This will retrieve the contents of the queues again, without processing
any tickboxes. In fact it will reset any checkboxes you've ticked.  You
can also use F5 to re-check. This will discard the state of any
checkboxes on the list.

To modify the settings for the tool at any time, click the settings
button. When the settings dialog is open, the tool suspends its
countdown to the next scan event.

The tool stores its settings in
  %APPData%\Dino Chiesa\CleanModQueue\Config.xml

You can modify that XML file directly, but be careful.  The tool will
choke if you break the schema.  It's probably easier to make the changes
in the tool itself.

Security notice: If, within the settings dialog, you tell the tool to
store your reddit password, that password will be stored in "clear text"
in the xml file. Anyone with access to your AppData directory will be
able to see your reddit password. This may be ok with you.

-----

You can view a log of the automated actions the tool has taken.  Click
the "View Log" button to see that.

The tool stores the audit log in
  %APPData%\Dino Chiesa\CleanModQueue\AuditLog.xml

There's no "undo" button on the Audit Log form, but if you want to undo
an action, you can right-click on an item in the audit log, and
select "View Item".  This will pop a browser to the page for that
item, and there you can remove or approve the item manually.

You should clear the audit log periodically, to prevent plaque buildup.
The logs can get pretty large after a while. The tool never clears the
log automatically.


Bugs:
======================

See the full list at
http://cleanmodqueue.codeplex.com/workitem/list/advanced

 - comments are not sufficiently distinguished from posts in the visual
   display of the main form.
   http://cleanmodqueue.codeplex.com/workitem/32667

