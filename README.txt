Your goal is to build a small vertical slice "Bomber Man" style game game in All Out. 
You can implement the features however you see fit and add things you think might be cool or fun. 
Remember one of our main goals is to make games that are fun to watch on YouTube!

Getting Setup:
 1. Log in to the takehome account with the credentials provided in the intro email. 
 2. Make sure you have dotnet installed. Anything at or above version 7.0 should work. https://dotnet.microsoft.com/en-us/download
 3. Open the All Out editor at https://allout.game/create by pressing "Launch Editor"
 4. Open the starter project in the editor File > Open > Select the takehome folder

Task:
 - Implement round based gameplay with "waiting for players" and "active" game states
 - Players can place bombs on the ground which explode after a few seconds. Players caught in the blast of the bomb die 
 - The round is over when only one player remains
 - Show a victory screen when the round ends
 - Put players joining mid-round into a spectating mode while they wait

Notes:
 - We're investing in editor stability, but there are some scenarios where it crashes. Make sure to save often!
 - We've provided some demo assets, but you can also pull in any assets from our asset library at https://allout.game/admin/assets

Bonus Points:
 - Make use of the NetSync attribute. 
 - This is a newer engine feature that syncs fields (that are in a component on a networked entity) marked with it over an unreliable network channel.
 - Limit usage of RPCs
 - Use our effects system
 - Try bomb types with different explosion patterns

Resources:
 - View the docs: https://docs.allout.game 
 - Check out our example projects: github.com/All-Out-Games/examples
 - Reach out to us! This is a work in progress engine, and we understand some parts may be confusing to use. 

When done please zip up the project and send it over, or alternatively push the project to a GitHub repo we can access!

Looking forward to seeing what you make, and as always if you have any questions let us know!