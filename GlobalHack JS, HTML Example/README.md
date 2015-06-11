# Overview

LockerDome applications are made to be extremely flexible and powerful. The primary way that an application interacts with LockerDome is by creating app content, and controlling the rendering of it.

## Table of Contents
  * [Creating an App](#creating-an-app)
  * [Creating your first piece of app content](#creating-your-first-piece-of-app-content)
  * [Handling the hook from the app](#handling-the-hook-from-the-app)
  * [Client Side APIs](#client-side-apis)
    * [Initial Data](#initial-data)
    * [Arguments](#arguments)
    * [Functions](#functions)
  * [Server Side APIs](#server-side-apis)
    * [app_create_content](#app_create_content)
    * [app_destroy_content](#app_destroy_content)
    * [app_fetch_content](#app_fetch_content)
    * [app_update_content](#app_update_content)
    * [app_fetch_user_content](#app_fetch_user_content)
    * [app_get_account_name_and_slug](#app_get_account_name_and_slug)
    * [app_fetch_batch_data](#app_fetch_batch_data)
    * [Errors](#errors)

## Creating an App

In order to create an app on LockerDome, a user account must first be created. Follow these steps:

1. Ensure that all cookies for lockerdome.com are cleared because these will interfere with the other steps
2. Visit [the GlobalHack 4 test page](http://globalhack4.test.lockerdome.com)
3. Click on the blue "Join" button and create an account
4. Visit [the app creation page](http://globalhack4.test.lockerdome.com/app/create)
5. Enter a name for your application
6. Enter a URL that will serve as the base URL of iframes’ src attribute that LockerDome will generate to hand off the rendering of your app’s content to your server. Specify something that can accept a query string that will be json and url encoded because this URL will serve as the master controller for your app.
7. Submit the form. You should receive an email with some information about your app. Be sure to keep the app secret and id safe, you'll need them to perform API calls. 

## Creating your first piece of app content

Content creation will be handled by a RESTful API. As explained below, all serverside APIs are in the form 

```http://api.globalhack4.test.lockerdome.com/<api call>?<JSON encoded arguments>```

In order to create your first piece of content, initiate an HTTP GET request with the following URL:

```
http://api.globalhack4.test.lockerdome.com/app_create_content?{"app_id":your_app_id,"app_secret":"your_app_secret","app_data":{"fun":"times"},"name":"Some App Content","text":"Short description of your content"}
```

The response you get back should contain an id and a status of true. If you visit your profile on the lockerdome staging environment, you should see a feed item with your created content. If you click on it, it will open an overlay that creates an iframe that calls over to the server hook you setup when you created your app.

## Handling the hook from the app

If your server that should be handling the iframe request isn’t set up to handle that path yet, then you may have an iframe that has an error message of some sort. No worries, you just need to get your server to respond. If your app path was `http://example-ld-app.com/hook` then you will need to handle `http://example-ld-app.com/hook?<data>` requests. The simplest way to do this is to just return some basic HTML to start with.

Obviously, as you improve your app, it should utilize the data stored in the app content to control what it shows. For example, if you created a poll, you might store the options in the app data. If you look at the example above, you’ll see that we passed `{"fun":"times"}` as our app_data. You can use anything you want there that is valid JSON. There is a size limit, but it is on the order of a few KB. If you need to store more than that, you should use a database on your server.

## Embedding

In order to test embedding your app, you can use [jsfiddle](http://jsfiddle.net). You can retrieve your content's embed code by clicking the '...' at the bottom right of the content feed item. Paste your code in jsfiddle, and **make sure** you are *not* using SSL.  

## Client Side APIs

Your IFRAME will get certain information from LockerDome about its context. This will include the account id and a special app specific login token that the app can use to create app content on behalf of the user. Additionally, the app may request an increase in the height of its iframe and get updates if the user logs in while the app’s iframe is open. See the example and include the script file http://globalhack4.test.lockerdome.com/gh_app_platform.js

### Initial Data

When your App's `ui_url` is fetched, some information is also passed by lockerdome through the GET request. The `app_platform` script does a good job of reading this data for you. The app platform allows you to access a variable named `LD` (LockerDome). The LD variable contains the following:

| Field               | Type      | Description                                                                                     |
|---------------------|-----------|-------------------------------------------------------------------------------------------------|
| account_id          | int       | The ID of the LockerDome account accessing your app.                                            |
| app_id              | int       | Your app's `id`                                                                                 |
| ld_url              | String    | The active LockerDome URL. In most cases this is just "http://globalhack4.test.lockerdome.com"  |
| login_token         | String    | A login token for **your app** unique to the user accessing it.                                 |
| args                | Object    | Arguments passed by LockerDome to your app. See [Arguments](#arguments) below.                  |

### Arguments

In addition to passing information about the user to your app, LockerDome also passes arguments. These arguments can be used by your application to determine what content the user is trying to access, and what operation to run (as of now this is always `render_content`). The arguments are detailed below.

| Argument    | Type    | Description                                                                   |
|-------------|---------|-------------------------------------------------------------------------------|
| id          | int     | The content ID being accessed by the user.                                    |
| op          | String  | The desired operation of your application (always `render_content` as of now) |


### Functions

The LockerDome app platform also includes a few client-side JavaScript functions. These functions allow your iframe to communicate with the main LockerDome page that it's embedded on. Currently, the following functions have been implemented.

#### request_width

The `request_width` function allows you to change the width of your app's frame.

| Argument    | Type    | Description                                                                           |
|-------------|---------|---------------------------------------------------------------------------------------|
| new_width   | String  | The desired width of your frame. This accepts any valid CSS value (400px, 100%, etc.) |

#### request_height 

The `request_height` function allows you to change the height of your app's frame. 

| Argument    | Type    | Description                                                                             |
|-------------|---------|-----------------------------------------------------------------------------------------|
| new_height  | String  | The desired height of your frame. This accepts any valid CSS value (400px, 100%, etc.)  |

#### request_redirect

Since your app is embedded in an iframe, you won't be able to execute browser redirects. If necessary, LockerDome can do this for you. A call to the `request_redirect` function will redirect the user to a desired URL.

| Argument  | Type    | Description                                                                     |
|-----------|---------|---------------------------------------------------------------------------------|
| path      | String  | The desired redirect URL. This will be checked by LockerDome before execution.  |
## Server Side APIs

The server-sided app API calls currently consist of the following:

+ [app_create_content](#app_create_content)
+ [app_destroy_content](#app_destroy_content)
+ [app_fetch_content](#app_fetch_content)
+ [app_update_content](#app_update_content)
+ [app_fetch_user_content](#app_fetch_user_content)
+ [app_get_account_name_and_slug](#app_get_account_name_and_slug)
+ [app_fetch_batch_data](#app_fetch_batch_data)

All *app* API calls require the following parameters:

| Parameter   | Type    | Description                         |
|-------------|---------|-------------------------------------|
| app_id      | int     | Your app's `app_id`                 |
| app_secret  | String  | Your app's `app_secret`             |

For the sake of this tutorial let's assume the following values:

```javascript
{
  your_user_id: 0,
  app_id: 1337,
  app_secret: "bananas",
}
```

For the tutorial, we'll walk you through [app_create_content](#app_create_content), but you'll have to figure the rest out on your own.

### app_create_content

Allows your app to create and hang content.

This API call accepts the following parameters:

| Parameter   | Type    | Required      | Description                                                                                     |
|-------------|---------|---------------|-------------------------------------------------------------------------------------------------|
| name        | String  | No            | Your app content's title. Keep it concise                                                       |
| thumb_url   | String  | No            | An optional (but recommended) thumbnail url                                                     |
| text        | String  | No            | An optional alt-text for your content                                                           |
| app_data    | Object  | No            | Optional data passed to your app (unique)                                                       |
| created_by  | int     | No            | Optional id of the user creating the content                                                    |
| login_token | String  | if created_by | If created_by is set, an auth token allowing your app to publish content as the specified user  |

Let's make a sample request. This sample will be nicely formatted and won't be url-encoded, however, actual requests need to be url-encoded.

```json
http://api.globalhack4.test.lockerdome.com/app_create_content?
{
  "app_id": 1337,
  "app_secret": "bananas",
  "name": "Fun App Content",
  "thumb_url": "http://yourapp.com/app_thumb.png",
  "text": "Isn't my app great?",
  "app_data": {
    "data":"some_data"
  }
}
```

If all goes well, we should receive something along the lines of the following.

```json
{
  "status": true,
  "result": {
    "app_data": {
      "data": "some_data"
    },
    "app_id": 1337,
    "created_by": 0,
    "id": 42,
    "name": "Fun App Content",
    "text": "Isn't my app great?",
    "thumb_url": "http://yourapp.com/app_thumb.png"
  }
}
```

Let's take a look at the response. The first field is the status of the transaction. Hopefully it's true, if it's not, see [Handling Errors](#errors). The `result` object is what we're interested in here. With LockerDome API calls, `result` is the returned data. In our case, it's the object that we've just created. Let's take a look at the fields. We won't include fields that we passed in our request.  

| Field       | Type    | Description                                                                           |
|-------------|---------|---------------------------------------------------------------------------------------|
| created_by  | int     | The `id` of the user that created the content. This defaults to the owner of the app. |
| id          | int     | The `id` of your content. It wouldn't be a bad idea to store this somewhere.          |

### app_destroy_content

Allows your app to destroy content as long as it has permission.

##### Parameters:

| Parameter   | Type    | Required  | Description                                 |
|-------------|---------|-----------|---------------------------------------------|
| content_id  | int     | Yes       | The `id` of the content to be deleted       |

##### Sample request:
```json
http://api.globalhack4.test.lockerdome.com/app_create_content?
{
  "app_id": 1337,
  "app_secret": "bananas",
  "content_id": 42 
}
```

##### Sample response:
```json
{
  "status": true
}
```

### app_fetch_content

Allows your app to fetch information about any content posted on LockerDome.

##### Parameters:

| Parameter   | Type    | Required  | Description                                 |
|-------------|---------|-----------|---------------------------------------------|
| content_id  | int     | Yes       | The `id` of the content to be fetched       | 

##### Sample request:
```json
http://api.globalhack4.test.lockerdome.com/app_create_content?
{
  "app_id": 1337,
  "app_secret": "bananas",
  "content_id": 42
}
```

##### Sample response:
```json
{
  "status": true,
  "result": {
    "app_data": {
      "data": "some_data"
    },
    "app_id": 1337,
    "created_by": 0,
    "id": 42,
    "name": "Fun App Content",
    "text": "Isn't my app great?",
    "thumb_url": "http://yourapp.com/app_thumb.png",
    "type": "app_content"
  }
}
```

### app_update_content

Allows you to update content created by your app. This is useful if your app utilizes the `app_data` field. 

##### Parameters: 

| Parameter   | Type    | Required  | Description                                 |
|-------------|---------|-----------|---------------------------------------------|
| content_id  | int     | Yes       | The `id` of the content you're updating     |
| name        | String  | No        | A new name for your content                 |
| thumb_url   | String  | No        | A new thumbnail url for your content        |
| text        | String  | No        | New alt-text for your content               |
| app_data    | Object  | No        | Updated data for this app content           |

##### Sample request:

```json
http://api.globalhack4.test.lockerdome.com/app_update_content?
{
  "app_id": 1337,
  "app_secret": "bananas",
  "content_id": 42,
  "name": "Not so fun App Content",
  "text": "Apparently it wasn't as fun as I thought",
  "thumb_url": "http://yourapp.com/app_thumb_boring.png",
  "app_data": {
    "is_fun": false
  }
}
```

##### Sample response:
```json
{
  "status": true,
  "result": {
    "app_data": {
      "is_fun": false
    },
    "app_id": 1337,
    "created_by": 0,
    "id": 42,
    "name": "Not so fun App Content",
    "text": "Apparently it wasn't as fun as I thought",
    "thumb_url": "http://yourapp.com/app_thumb_boring.png"
  }
}
```

### app_fetch_user_content

Allows your app to fetch all content created by a specified user.

##### Parameters

| Parameter   | Type    | Required  | Description                                 |
|-------------|---------|-----------|---------------------------------------------|
| created_by  | int     | Yes       | The `id` of the user whose content to fetch |

##### Sample request: 
```json
http://api.globalhack4.test.lockerdome.com/app_fetch_user_content?
{
  "app_id": 1337,
  "app_secret": "bananas",
  "created_by": 0
}
```
##### Sample response: 
```json
{
  "status": true,
  "result": {
    "42": {
      "app_data": {
        "data": "some_data"
      },
      "app_id": 1337,
      "created_by": 0,
      "id": 42,
      "name": "Fun App Content",
      "text": "Isn't my app great?",
      "thumb_url": "http://yourapp.com/app_thumb.png",
      "type": "app_content"
    },
    "314159": {
      "app_data": {
        "is_fun": false
      },
      "app_id": 1337,
      "created_by": 0,
      "id": 314159,
      "name": "Not so fun App Content",
      "text": "Apparently it wasn't as fun as I thought",
      "thumb_url": "http://yourapp.com/app_thumb_boring.png",
      "type": "app_content"
    }
  }
}
```

### app_get_account_name_and_slug

Allows your app to fetch the account name and slug of specified user(s).

##### Parameters

| Parameter   | Type    | Required  | Description                                 |
|-------------|---------|-----------|---------------------------------------------|
| account_ids | int[]   | Yes       | An array of account `id`s to fetch          |

##### Sample request: 
```json
http://api.globalhack4.test.lockerdome.com/app_get_account_name_and_slug?
{
  "app_id": 1337,
  "app_secret": "bananas",
  "account_ids": [ 0, 1 ]
}
```
##### Sample response:
```json
{
  "status": true,
  "result": {
    "0": {
      "id": 0,
      "name": "Harry Gallagher",
      "slug": "harry"
    },
    "1": {
      "id": 1,
      "name": "Gabe Lozano",
      "slug": "gabe"
    }
  }
}
```

### app_fetch_batch_data

##### Parameters

| Parameter   | Type    | Required  | Description                                 |
|-------------|---------|-----------|---------------------------------------------|
| content_ids | int[]   | Yes       | An array of content `id`s to fetch          |

##### Sample request:
```json
http://api.globalhack4.test.lockerdome.com/app_fetch_batch_data?
{
  "app_id": 1337,
  "app_secret": "bananas",
  "content_ids": [ 42, 314159 ]
}
```

##### Sample response: 
```json
{
  "status": true,
  "result": {
    "42": {
      "app_data": {
        "data": "some_data"
      },
      "app_id": 1337,
      "created_by": 0,
      "id": 42,
      "name": "Fun App Content",
      "text": "Isn't my app great?",
      "thumb_url": "http://yourapp.com/app_thumb.png",
      "type": "app_content"
    },
    "314159": {
      "app_data": {
        "is_fun": false
      },
      "app_id": 1337,
      "created_by": 0,
      "id": 314159,
      "name": "Not so fun App Content",
      "text": "Apparently it wasn't as fun as I thought",
      "thumb_url": "http://yourapp.com/app_thumb_boring.png",
      "type": "app_content"
    }
  }
}
```

### Errors

When making server side API calls, you can run into a few errors. We try to be as helpful as possible with the error message you receive when an operation fails. Errors are returned in this format:
```json
{
  "status": false,
  "error_message": "A short message detailing the error. It'll help you resolve it."
}
```
In your app, you should always check the `status` field of a response before trying to process it. `status` will be true if the operation was a success, and false if it fails.