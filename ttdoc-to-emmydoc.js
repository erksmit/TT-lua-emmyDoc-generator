function removeLineBreaks(string) {
    var filteredString = "";
    for(let j = 0; j < string.length; j++ ) 
        if(!(string[j] == '\n'))
            filteredString += string[j];
        else
            filteredString += ' '
    return filteredString.trim()
}

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
        let optional = li.getElementsByTagName('em').length !== 0

        let paramText = name
        if(optional)
            paramText += '?'
        paramText += ': '
        paramText += type
        parameters.push(paramText)
    }
    return parameters.join(', ')
}

function getDescription(descriptionElement) {
    let description = descriptionElement.textContent;
    let descEnd = description.search(/Returns:|Parameters:|New feature:/gm)
    if(descEnd !== -1)
        description = description.substring(0, descEnd)
    return removeLineBreaks(description)
}

function getReturn(descriptionElement) {
    let returnHeader
    let unfilteredReturnHeaders = descriptionElement.getElementsByTagName('h3')
    for (const header of unfilteredReturnHeaders) {
        if(header.textContent === 'Returns:') {
            returnHeader = header
            break
        }
    }

    if(returnHeader !== undefined) {
        let returnListIndex = Array.from(descriptionElement.children).indexOf(returnHeader) + 1
        let returnList = descriptionElement.children[returnListIndex]
        let returnTypeElements = returnList.getElementsByClassName('types')
        let returnTypes = []
        for (const returnElement of returnTypeElements)
            returnTypes.push(returnElement.textContent)
        return returnTypes.join(', ')
    }
}

function getdocs() {
    let output = ''
    let moduleName = document.getElementsByTagName('code')[0].textContent
    let moduleDesc = document.getElementById('content').getElementsByTagName('p')[0].textContent
    moduleDesc = removeLineBreaks(moduleDesc)
    output += '---@type ' + moduleName + '\n'
    output += `${moduleName} = ${moduleName}\n`
    output += `---@class ${moduleName} ${moduleDesc}\n`

    let functionTables = document.getElementsByClassName('function')
    for (const table of functionTables) {
        let titles = table.getElementsByTagName('dt')
        let descriptions = table.getElementsByTagName('dd')
        for (let i = 0; i < titles.length; i++) {
            let title = titles[i].children[0].getAttribute('name').split(':').pop()
            let isFunction = titles[i].textContent.includes('(')

            let descriptionElement = descriptions[i]
            let description = getDescription(descriptionElement)
            let parameters = null
            let returnTypes = null
            if(isFunction) {
                parameters = getParameters(descriptionElement)
                returnTypes = getReturn(descriptionElement)
            }

            //write the emmydocs line
            let line = '---@field '
            line += title
            if(isFunction) {
                line += ' fun('
                line += parameters
                line += ')'
                if(returnTypes !== undefined)
                    line += ': ' + returnTypes
            }
            else
                line += ' any'
            line += ' ' + description
            output += line + '\n'
        }
    }
    output = output.replace(/\bbool\b/g, 'boolean')
    output = output.replace(/\banything\b/g, 'any')
    output = output.replace(/\bint\b|\binteger\b|\bfloat\b/g, 'number')
    console.log(output)
}
getdocs()