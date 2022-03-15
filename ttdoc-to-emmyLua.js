function removeLineBreaks(string) {
    var filteredString = "";
    for(let j = 0; j < string.length; j++ ) 
        if(!(string[j] == '\n'))
            filteredString += string[j];
        else
            filteredString += ' '
    return filteredString.trim()
}

function removeExcessSpaces(string) {
    return string.replace(/\s+/g,' ')
}

//retrieve the parameters of a function
function getParameters(descriptionElement) {
    let parameters = []
    let lis = descriptionElement.getElementsByTagName('li')
    for (const li of lis) {
        let nameElement = li.getElementsByClassName('parameter')[0]
        if(nameElement === undefined)
            continue
        let typeElements = li.getElementsByClassName('type')
        let name = nameElement.textContent
        let type = 'any'
        if(typeElements.length !== 0) {
            type = typeElements[0].textContent
        }
        let param = {}
        param.description = ''
        let optional = li.getElementsByTagName('em').length !== 0
        for (var i = 0; i < li.childNodes.length; ++i) {
            if (li.childNodes[i].nodeType == Node.TEXT_NODE) {
                let text = removeLineBreaks(li.childNodes[i].textContent)
                if (text === type)
                    continue
                if (text === "")
                    continue
                if (text == ')')
                    continue
                param.description = text.replace('(', '')
                break
            }
        }
        param.name = name
        param.type = type
        param.optional = optional
        parameters.push(param)
    }
    return parameters
}

//retrieve the description of a function
function getDescription(descriptionElement) {
    let description = descriptionElement.textContent;
    let descEnd = description.search(/Returns:|Parameters:|New feature:/gm)
    if(descEnd !== -1)
        description = description.substring(0, descEnd)
    return removeLineBreaks(description)
}

//retrieve the return value of a function
function getReturn(descriptionElement) {
    let returnHeader
    let unfilteredReturnHeaders = descriptionElement.getElementsByTagName('h3')
    for (const header of unfilteredReturnHeaders) {
        if(header.textContent === 'Returns:') {
            returnHeader = header
            break
        }
    }

    let result = null
    if(returnHeader !== undefined) {
        result = ''
        let returnListIndex = Array.from(descriptionElement.children).indexOf(returnHeader) + 1
        let returnList = descriptionElement.children[returnListIndex]
        let returnListItems = returnList.getElementsByTagName('li')
        if (returnListItems.length > 0) 
            for (const returnItem of returnListItems) {
                let type = returnItem.getElementsByClassName('type')[0].textContent
                let desc = ''
                for (var i = 0; i < returnItem.childNodes.length; ++i)
                    if (returnItem.childNodes[i].nodeType === Node.TEXT_NODE)
                        desc = removeExcessSpaces(removeLineBreaks(returnItem.childNodes[i].textContent))
                result += `---@return ${type} ${desc}\n`
            }
        else {
            try {
            let type = returnList.getElementsByClassName('type')[0].textContent
            let desc = ''
            for (var i = 0; i < returnList.childNodes.length; ++i)
                if (returnList.childNodes[i].nodeType === Node.TEXT_NODE)
                    desc = removeExcessSpaces(removeLineBreaks(returnList.childNodes[i].textContent))
            result += `---@return ${type} r ${desc}\n`
            }
            catch {
                result += `---@return any r ${removeExcessSpaces(removeLineBreaks(returnList.textContent))}\n`
            }
        }
    }
    return result
}

function getdocs() {
    let output = ''
    let moduleName = document.getElementsByTagName('code')[0].textContent
    let moduleDesc = document.getElementById('content').getElementsByTagName('p')[0].textContent
    moduleDesc = removeLineBreaks(moduleDesc)
    output += `---@class ${moduleName.toLowerCase()} : ${moduleName}\n`
    output += `---@class ${moduleName} ${moduleDesc}\n`

    let functionString = ''
    let fieldString = ''
    let functionTables = document.getElementsByClassName('function')
    for (const table of functionTables) {
        let titles = table.getElementsByTagName('dt')
        let descriptions = table.getElementsByTagName('dd')
        for (let i = 0; i < titles.length; i++) {
            let title = titles[i].children[0].getAttribute('name')
            if (!title.includes(':'))
                title = moduleName + '.' + title
            let isFunction = titles[i].textContent.includes('(')
            let descriptionElement = descriptions[i]
            let description = removeExcessSpaces(getDescription(descriptionElement))
            let parameters = null
            let returns = null
            if(isFunction) {
                parameters = getParameters(descriptionElement)
                returns = getReturn(descriptionElement)
            }

            //write the emmydocs lines
            let line = ''
            if(isFunction) {
                line += `-- ${description}\n`
                let paramStrings = []
                for (const param of parameters) {
                    line += `---@param ${param.name}`
                    let string = param.name
                    if (param.optional) {
                        line += '?'
                    }
                    line += ` ${param.type} ${param.description}\n`
                    paramStrings.push(string)
                }
                if(returns !== null)
                    line += returns
                let paramString = paramStrings.join(', ')
                line += `function ${title}(${paramString}) end`
                functionString += line + '\n\n'
            }
            else {
                title = title.split('.').pop()
                line += `---@field ${title} any ${description}`
                fieldString += line + '\n'
            }
        }
    }
    output += fieldString
    output += `${moduleName} = {} \n`
    output += functionString
    output = output.replace(/\bbool\b/g, 'boolean')
    output = output.replace(/\banything\b/g, 'any')
    output = output.replace(/\bint\b|\binteger\b|\bfloat\b/g, 'number')
    console.log(output)
}
getdocs()